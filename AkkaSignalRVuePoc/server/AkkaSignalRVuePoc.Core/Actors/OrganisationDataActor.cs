using Akka.Actor;
using Akka.Event;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Core.Data;
using AkkaSignalRVuePoc.Data;
using AkkaSignalRVuePoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AkkaSignalRVuePoc.Core.Actors;

public sealed class OrganisationDataActor : ReceiveActor
{
    private readonly IDbContextFactory<CatalogDbContext> _dbContextFactory;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public OrganisationDataActor(IDbContextFactory<CatalogDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;

        ReceiveAsync<GetAllOrganisationsQuery>(HandleGetAll);
        ReceiveAsync<GetOrganisationByIdQuery>(HandleGetById);
        ReceiveAsync<CreateOrganisationCommand>(HandleCreate);
    }

    public static Props Props(IDbContextFactory<CatalogDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new OrganisationDataActor(dbContextFactory));

    private async Task HandleGetAll(GetAllOrganisationsQuery query)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var organisations = await db.Organisations
            .AsNoTracking()
            .OrderBy(organisation => organisation.Name)
            .ToListAsync();

        Sender.Tell(new GetAllOrganisationsResult(organisations.ConvertAll(CatalogEntityMapper.ToDto)));
    }

    private async Task HandleGetById(GetOrganisationByIdQuery query)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var organisation = await db.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == query.Id);

        Sender.Tell(new GetOrganisationByIdResult(
            organisation is null ? null : CatalogEntityMapper.ToDto(organisation)));
    }

    private async Task HandleCreate(CreateOrganisationCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            Sender.Tell(new Status.Failure(new ArgumentException("Name is required.", nameof(command.Name))));
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
        Sender.Tell(new CreateOrganisationResult(CatalogEntityMapper.ToDto(organisation)));
    }
}
