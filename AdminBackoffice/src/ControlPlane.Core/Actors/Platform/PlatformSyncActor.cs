using Akka.Actor;
using Akka.Event;
using ControlPlane.Application.Ports;
using ControlPlane.Application.Services;
using ControlPlane.Contracts.Messages.Platform;
using Platform.ControlPlane.Client;
using Platform.Serilog.Logging.Akka;
using Platform.Serilog.Logging.Correlation;

namespace ControlPlane.Core.Actors.Platform;

/// <summary>
/// Pushes tenant configuration to the v2 platform runtime (HTTP boundary).
/// </summary>
public sealed class PlatformSyncActor : PlatformReceiveActor
{
    private readonly IPlatformConfigurationClient _platformClient;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public PlatformSyncActor(IPlatformConfigurationClient platformClient)
    {
        _platformClient = platformClient;
        RegisterEnvelopeHandler();
        ReceiveCorrelated<PushTenantConfigCommand>(HandlePush);
    }

    public static Props Props(IPlatformConfigurationClient platformClient) =>
        Akka.Actor.Props.Create(() => new PlatformSyncActor(platformClient));

    private void HandlePush(PushTenantConfigCommand command, CorrelationFlow flow, IActorRef sender)
    {
        using CorrelationScope scope = flow.BeginScope();
        try
        {
            var payload = TenantRecordMapper.ToConfigurationPayload(command.Tenant);
            _platformClient.PushTenantConfigurationAsync(payload, CancellationToken.None).GetAwaiter().GetResult();
            _log.Info("Pushed tenant configuration for {0} ({1})", command.Tenant.TenantId, command.Tenant.Slug);
            sender.Tell(new PushTenantConfigResult(true, null));
        }
        catch (Exception exception)
        {
            _log.Warning(exception, "Failed to push tenant configuration for {0}", command.Tenant.TenantId);
            sender.Tell(new PushTenantConfigResult(false, exception.Message));
        }
    }
}
