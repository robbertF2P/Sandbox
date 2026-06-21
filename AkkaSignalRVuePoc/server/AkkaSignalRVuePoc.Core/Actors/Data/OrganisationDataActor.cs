using Akka.Actor;
using Akka.Event;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Core.Data;
using AkkaSignalRVuePoc.Data;
using AkkaSignalRVuePoc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Platform.Serilog.Logging.Correlation;

namespace AkkaSignalRVuePoc.Core.Actors.Data;

public sealed class OrganisationDataActor : ReceiveActor
{
    private readonly IDbContextFactory<CatalogDbContext> _dbContextFactory;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public OrganisationDataActor(IDbContextFactory<CatalogDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        ReceiveAsync<CorrelatedMessageEnvelope>(DispatchAsync);
    }

    public static Props Props(IDbContextFactory<CatalogDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new OrganisationDataActor(dbContextFactory));

    private async Task DispatchAsync(CorrelatedMessageEnvelope envelope)
    {
        var sender = Sender;
        var flow = new CorrelationFlow(envelope.CorrelationId, envelope.UseCase, envelope.CausationId);
        using CorrelationScope scope = flow.BeginScope();

        switch (envelope.Message)
        {
            case GetAllOrganisationsQuery query:
                await HandleGetAllAsync(query, sender);
                break;
            case GetOrganisationByIdQuery query:
                await HandleGetByIdAsync(query, sender);
                break;
            case CreateOrganisationCommand command:
                await HandleCreateAsync(command, sender);
                break;
            default:
                Unhandled(envelope);
                break;
        }
    }

    private async Task HandleGetAllAsync(GetAllOrganisationsQuery query, IActorRef sender)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var organisations = await db.Organisations
            .AsNoTracking()
            .OrderBy(organisation => organisation.Name)
            .ToListAsync();

        sender.Tell(new GetAllOrganisationsResult(organisations.ConvertAll(CatalogEntityMapper.ToDto)));
    }

    private async Task HandleGetByIdAsync(GetOrganisationByIdQuery query, IActorRef sender)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var organisation = await db.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == query.Id);

        sender.Tell(new GetOrganisationByIdResult(
            organisation is null ? null : CatalogEntityMapper.ToDto(organisation)));
    }

    private async Task HandleCreateAsync(CreateOrganisationCommand command, IActorRef sender)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            sender.Tell(new Status.Failure(new ArgumentException("Name is required.", nameof(command.Name))));
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var organisation = new Organisation
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Organisations.Add(organisation);
        await db.SaveChangesAsync();

        _log.Info("Created organisation {OrganisationId} ({Name})", organisation.Id, organisation.Name);
        sender.Tell(new CreateOrganisationResult(CatalogEntityMapper.ToDto(organisation)));
    }
}
