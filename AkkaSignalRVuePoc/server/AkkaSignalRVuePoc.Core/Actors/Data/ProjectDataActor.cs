using Akka.Actor;
using Akka.Event;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Core.Data;
using AkkaSignalRVuePoc.Data;
using AkkaSignalRVuePoc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Platform.Serilog.Logging.Correlation;

namespace AkkaSignalRVuePoc.Core.Actors.Data;

public sealed class ProjectDataActor : ReceiveActor
{
    private readonly IDbContextFactory<CatalogDbContext> _dbContextFactory;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public ProjectDataActor(IDbContextFactory<CatalogDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        ReceiveAsync<CorrelatedMessageEnvelope>(DispatchAsync);
    }

    public static Props Props(IDbContextFactory<CatalogDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new ProjectDataActor(dbContextFactory));

    private async Task DispatchAsync(CorrelatedMessageEnvelope envelope)
    {
        var sender = Sender;
        var flow = new CorrelationFlow(envelope.CorrelationId, envelope.UseCase, envelope.CausationId);
        using CorrelationScope scope = flow.BeginScope();

        switch (envelope.Message)
        {
            case GetAllProjectsQuery query:
                await HandleGetAllAsync(query, sender);
                break;
            case GetProjectByIdQuery query:
                await HandleGetByIdAsync(query, sender);
                break;
            case GetProjectsByOrganisationQuery query:
                await HandleGetByOrganisationAsync(query, sender);
                break;
            case CreateProjectCommand command:
                await HandleCreateAsync(command, sender);
                break;
            case UpdateProjectCommand command:
                await HandleUpdateAsync(command, sender);
                break;
            case DeleteProjectCommand command:
                await HandleDeleteAsync(command, sender);
                break;
            default:
                Unhandled(envelope);
                break;
        }
    }

    private async Task HandleGetAllAsync(GetAllProjectsQuery query, IActorRef sender)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var projects = await db.Projects
            .AsNoTracking()
            .OrderBy(project => project.Name)
            .ToListAsync();

        sender.Tell(new GetAllProjectsResult(projects.ConvertAll(CatalogEntityMapper.ToDto)));
    }

    private async Task HandleGetByIdAsync(GetProjectByIdQuery query, IActorRef sender)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == query.Id);

        sender.Tell(new GetProjectByIdResult(
            project is null ? null : CatalogEntityMapper.ToDto(project)));
    }

    private async Task HandleGetByOrganisationAsync(GetProjectsByOrganisationQuery query, IActorRef sender)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var organisationExists = await db.Organisations
            .AsNoTracking()
            .AnyAsync(organisation => organisation.Id == query.OrganisationId);

        if (!organisationExists)
        {
            sender.Tell(new GetProjectsByOrganisationResult(query.OrganisationId, false, []));
            return;
        }

        var projects = await db.Projects
            .AsNoTracking()
            .Where(project => project.OrganisationId == query.OrganisationId)
            .OrderBy(project => project.Name)
            .ToListAsync();

        sender.Tell(new GetProjectsByOrganisationResult(
            query.OrganisationId,
            true,
            projects.ConvertAll(CatalogEntityMapper.ToDto)));
    }

    private async Task HandleCreateAsync(CreateProjectCommand command, IActorRef sender)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            sender.Tell(new Status.Failure(new ArgumentException("Name is required.", nameof(command.Name))));
            return;
        }

        if (command.OrganisationId == Guid.Empty)
        {
            sender.Tell(new Status.Failure(
                new ArgumentException("OrganisationId is required.", nameof(command.OrganisationId))));
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var organisationExists = await db.Organisations
            .AsNoTracking()
            .AnyAsync(organisation => organisation.Id == command.OrganisationId);

        if (!organisationExists)
        {
            sender.Tell(new CreateProjectResult(false, null));
            return;
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            OrganisationId = command.OrganisationId,
            Name = command.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(command.Description)
                ? null
                : command.Description.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Projects.Add(project);
        await db.SaveChangesAsync();

        _log.Info("Created project {ProjectId} ({Name})", project.Id, project.Name);
        sender.Tell(new CreateProjectResult(true, CatalogEntityMapper.ToDto(project)));
    }

    private async Task HandleUpdateAsync(UpdateProjectCommand command, IActorRef sender)
    {
        if (command.Id == Guid.Empty)
        {
            sender.Tell(new Status.Failure(new ArgumentException("Id is required.", nameof(command.Id))));
            return;
        }

        var hasName = !string.IsNullOrWhiteSpace(command.Name);
        var hasDescription = command.Description is not null;
        if (!hasName && !hasDescription)
        {
            sender.Tell(new Status.Failure(
                new ArgumentException("At least one of Name or Description must be provided.")));
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var project = await db.Projects.FirstOrDefaultAsync(entity => entity.Id == command.Id);
        if (project is null)
        {
            sender.Tell(new UpdateProjectResult(false, null));
            return;
        }

        if (hasName)
        {
            project.Name = command.Name!.Trim();
        }

        if (hasDescription)
        {
            project.Description = string.IsNullOrWhiteSpace(command.Description)
                ? null
                : command.Description.Trim();
        }

        await db.SaveChangesAsync();

        _log.Info("Updated project {ProjectId} ({Name})", project.Id, project.Name);
        sender.Tell(new UpdateProjectResult(true, CatalogEntityMapper.ToDto(project)));
    }

    private async Task HandleDeleteAsync(DeleteProjectCommand command, IActorRef sender)
    {
        if (command.Id == Guid.Empty)
        {
            sender.Tell(new Status.Failure(new ArgumentException("Id is required.", nameof(command.Id))));
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var project = await db.Projects.FirstOrDefaultAsync(entity => entity.Id == command.Id);
        if (project is null)
        {
            sender.Tell(new DeleteProjectResult(false, null));
            return;
        }

        var dto = CatalogEntityMapper.ToDto(project);
        db.Projects.Remove(project);
        await db.SaveChangesAsync();

        _log.Info("Deleted project {ProjectId} ({Name})", project.Id, project.Name);
        sender.Tell(new DeleteProjectResult(true, dto));
    }
}
