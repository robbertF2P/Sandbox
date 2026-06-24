using Akka.Actor;
using ControlPlane.Contracts.Messages.Persist;
using ControlPlane.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Platform.Serilog.Logging.Akka;
using Platform.Serilog.Logging.Correlation;

namespace ControlPlane.Core.Actors.Persist;

/// <summary>
/// Sole EF Core writer for control-plane tenant registry (actor boundary).
/// </summary>
public sealed class TenantPersistActor : PlatformReceiveActor
{
    private readonly IDbContextFactory<ControlPlaneDbContext> _dbContextFactory;

    public TenantPersistActor(IDbContextFactory<ControlPlaneDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        RegisterEnvelopeHandler();
        ReceiveCorrelated<GetTenantBySlugQuery>(HandleGetBySlug);
        ReceiveCorrelated<GetTenantByIdQuery>(HandleGetById);
        ReceiveCorrelated<AddTenantCommand>(HandleAdd);
        ReceiveCorrelated<UpdateTenantCommand>(HandleUpdate);
    }

    public static Props Props(IDbContextFactory<ControlPlaneDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new TenantPersistActor(dbContextFactory));

    private void HandleGetBySlug(GetTenantBySlugQuery query, CorrelationFlow flow, IActorRef sender)
    {
        using CorrelationScope scope = flow.BeginScope();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var repository = new EfTenantRepository(dbContext);
        var tenant = repository.GetBySlugAsync(query.Slug, CancellationToken.None).GetAwaiter().GetResult();
        sender.Tell(new GetTenantBySlugResult(tenant));
    }

    private void HandleGetById(GetTenantByIdQuery query, CorrelationFlow flow, IActorRef sender)
    {
        using CorrelationScope scope = flow.BeginScope();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var repository = new EfTenantRepository(dbContext);
        var tenant = repository.GetByIdAsync(query.TenantId, CancellationToken.None).GetAwaiter().GetResult();
        sender.Tell(new GetTenantByIdResult(tenant));
    }

    private void HandleAdd(AddTenantCommand command, CorrelationFlow flow, IActorRef sender)
    {
        using CorrelationScope scope = flow.BeginScope();
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var repository = new EfTenantRepository(dbContext);
            var tenant = repository.AddAsync(command.Tenant, CancellationToken.None).GetAwaiter().GetResult();
            sender.Tell(new AddTenantResult(true, tenant, null));
        }
        catch (Exception exception)
        {
            sender.Tell(new AddTenantResult(false, null, exception.Message));
        }
    }

    private void HandleUpdate(UpdateTenantCommand command, CorrelationFlow flow, IActorRef sender)
    {
        using CorrelationScope scope = flow.BeginScope();
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var repository = new EfTenantRepository(dbContext);
            var tenant = repository.UpdateAsync(command.Tenant, CancellationToken.None).GetAwaiter().GetResult();
            sender.Tell(new UpdateTenantResult(true, tenant, null));
        }
        catch (Exception exception)
        {
            sender.Tell(new UpdateTenantResult(false, null, exception.Message));
        }
    }
}
