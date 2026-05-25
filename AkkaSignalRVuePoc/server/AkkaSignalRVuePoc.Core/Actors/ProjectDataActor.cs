using Akka.Actor;
using Akka.Event;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Core.Data;
using AkkaSignalRVuePoc.Data;
using AkkaSignalRVuePoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AkkaSignalRVuePoc.Core.Actors;

public sealed class ProjectDataActor : ReceiveActor
{
    private readonly IDbContextFactory<CatalogDbContext> _dbContextFactory;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public ProjectDataActor(IDbContextFactory<CatalogDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;

        ReceiveAsync<GetAllProjectsQuery>(HandleGetAll);
        ReceiveAsync<GetProjectByIdQuery>(HandleGetById);
        ReceiveAsync<GetProjectsByOrganisationQuery>(HandleGetByOrganisation);
        ReceiveAsync<CreateProjectCommand>(HandleCreate);
        ReceiveAsync<UpdateProjectCommand>(HandleUpdate);
        ReceiveAsync<DeleteProjectCommand>(HandleDelete);
    }

    public static Props Props(IDbContextFactory<CatalogDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new ProjectDataActor(dbContextFactory));

    private async Task HandleGetAll(GetAllProjectsQuery query)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var projects = await db.Projects
            .AsNoTracking()
            .OrderBy(project => project.Name)
            .ToListAsync();

        Sender.Tell(new GetAllProjectsResult(projects.ConvertAll(CatalogEntityMapper.ToDto)));
    }

    private async Task HandleGetById(GetProjectByIdQuery query)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == query.Id);

        Sender.Tell(new GetProjectByIdResult(
            project is null ? null : CatalogEntityMapper.ToDto(project)));
    }

    private async Task HandleGetByOrganisation(GetProjectsByOrganisationQuery query)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var organisationExists = await db.Organisations
            .AsNoTracking()
            .AnyAsync(organisation => organisation.Id == query.OrganisationId);

        if (!organisationExists)
        {
            Sender.Tell(new GetProjectsByOrganisationResult(query.OrganisationId, false, []));
            return;
        }

        var projects = await db.Projects
            .AsNoTracking()
            .Where(project => project.OrganisationId == query.OrganisationId)
            .OrderBy(project => project.Name)
            .ToListAsync();

        Sender.Tell(new GetProjectsByOrganisationResult(
            query.OrganisationId,
            true,
            projects.ConvertAll(CatalogEntityMapper.ToDto)));
    }

    private async Task HandleCreate(CreateProjectCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            Sender.Tell(new Status.Failure(new ArgumentException("Name is required.", nameof(command.Name))));
            return;
        }

        if (command.OrganisationId == Guid.Empty)
        {
            Sender.Tell(new Status.Failure(
                new ArgumentException("OrganisationId is required.", nameof(command.OrganisationId))));
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var organisationExists = await db.Organisations
            .AsNoTracking()
            .AnyAsync(organisation => organisation.Id == command.OrganisationId);

        if (!organisationExists)
        {
            Sender.Tell(new CreateProjectResult(false, null));
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
        Sender.Tell(new CreateProjectResult(true, CatalogEntityMapper.ToDto(project)));
    }

    private async Task HandleUpdate(UpdateProjectCommand command)
    {
        if (command.Id == Guid.Empty)
        {
            Sender.Tell(new Status.Failure(new ArgumentException("Id is required.", nameof(command.Id))));
            return;
        }

        var hasName = !string.IsNullOrWhiteSpace(command.Name);
        var hasDescription = command.Description is not null;
        if (!hasName && !hasDescription)
        {
            Sender.Tell(new Status.Failure(
                new ArgumentException("At least one of Name or Description must be provided.")));
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var project = await db.Projects.FirstOrDefaultAsync(entity => entity.Id == command.Id);
        if (project is null)
        {
            Sender.Tell(new UpdateProjectResult(false, null));
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
        Sender.Tell(new UpdateProjectResult(true, CatalogEntityMapper.ToDto(project)));
    }

    private async Task HandleDelete(DeleteProjectCommand command)
    {
        if (command.Id == Guid.Empty)
        {
            Sender.Tell(new Status.Failure(new ArgumentException("Id is required.", nameof(command.Id))));
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var project = await db.Projects.FirstOrDefaultAsync(entity => entity.Id == command.Id);
        if (project is null)
        {
            Sender.Tell(new DeleteProjectResult(false, null));
            return;
        }

        var dto = CatalogEntityMapper.ToDto(project);
        db.Projects.Remove(project);
        await db.SaveChangesAsync();

        _log.Info("Deleted project {ProjectId} ({Name})", project.Id, project.Name);
        Sender.Tell(new DeleteProjectResult(true, dto));
    }
}
