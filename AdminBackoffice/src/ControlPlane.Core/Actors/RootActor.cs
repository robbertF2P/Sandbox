using Akka.Actor;
using ControlPlane.Application.Ports;
using ControlPlane.Contracts.Messages.Provisioning;
using ControlPlane.Core.Actors.Persist;
using ControlPlane.Core.Actors.Platform;
using ControlPlane.Core.Actors.Provisioning;
using ControlPlane.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Platform.ControlPlane.Client;
using Platform.Serilog.Logging.Akka;

namespace ControlPlane.Core.Actors;

public sealed class RootActor : PlatformReceiveActor
{
    private readonly IDbContextFactory<ControlPlaneDbContext> _dbContextFactory;
    private readonly IPlatformConfigurationClient _platformClient;
    private IActorRef _provisioningManager = ActorRefs.Nobody;

    public RootActor(
        IDbContextFactory<ControlPlaneDbContext> dbContextFactory,
        IPlatformConfigurationClient platformClient)
    {
        _dbContextFactory = dbContextFactory;
        _platformClient = platformClient;
    }

    public static Props Props(
        IDbContextFactory<ControlPlaneDbContext> dbContextFactory,
        IPlatformConfigurationClient platformClient) =>
        Akka.Actor.Props.Create(() => new RootActor(dbContextFactory, platformClient));

    protected override void PreStart()
    {
        var persistActor = Context.ActorOf(TenantPersistActor.Props(_dbContextFactory), "tenant-persist");
        var platformSyncActor = Context.ActorOf(PlatformSyncActor.Props(_platformClient), "platform-sync");
        _provisioningManager = Context.ActorOf(
            TenantProvisioningManagerActor.Props(persistActor, platformSyncActor),
            "tenant-provisioning-manager");
        Ready();
    }

    private void Ready()
    {
        RegisterEnvelopeHandler();
        ReceiveCorrelated<ProvisionTenantCommand>((command, flow) =>
            _provisioningManager.Forward(flow.Wrap(command)));
        ReceiveCorrelated<SyncTenantCommand>((command, flow) =>
            _provisioningManager.Forward(flow.Wrap(command)));
    }
}
