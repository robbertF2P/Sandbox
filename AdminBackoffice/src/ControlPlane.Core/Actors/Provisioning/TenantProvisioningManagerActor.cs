using Akka.Actor;
using Akka.Event;
using ControlPlane.Application.Services;
using ControlPlane.Contracts.Messages.Persist;
using ControlPlane.Contracts.Messages.Platform;
using ControlPlane.Contracts.Messages.Provisioning;
using Platform.ControlPlane.Contracts;
using Platform.Serilog.Logging.Akka;
using Platform.Serilog.Logging.Correlation;

namespace ControlPlane.Core.Actors.Provisioning;

/// <summary>
/// Orchestrates tenant provisioning: persist → platform sync → status update.
/// </summary>
public sealed class TenantProvisioningManagerActor : PlatformReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private IActorRef _persistActor = ActorRefs.Nobody;
    private IActorRef _platformSyncActor = ActorRefs.Nobody;

    public TenantProvisioningManagerActor(IActorRef persistActor, IActorRef platformSyncActor)
    {
        _persistActor = persistActor;
        _platformSyncActor = platformSyncActor;
        Become(Ready);
    }

    public static Props Props(IActorRef persistActor, IActorRef platformSyncActor) =>
        Akka.Actor.Props.Create(() => new TenantProvisioningManagerActor(persistActor, platformSyncActor));

    private void Ready()
    {
        RegisterEnvelopeHandler();
        ReceiveCorrelated<ProvisionTenantCommand>(StartProvision);
        ReceiveCorrelated<SyncTenantCommand>(StartSync);
    }

    private void StartProvision(ProvisionTenantCommand command, CorrelationFlow flow, IActorRef sender)
    {
        try
        {
            ValidateRequest(command.Request);
            var normalizedSlug = command.Request.Slug.Trim().ToLowerInvariant();
            _persistActor.Tell(flow.WrapChild(new GetTenantBySlugQuery(normalizedSlug), "Tenant.LookupSlug"), Self);
            Become(() => WaitForSlugCheck(command.Request, normalizedSlug, flow, sender));
        }
        catch (ArgumentException exception)
        {
            sender.Tell(new ProvisionTenantResult(false, null, exception.Message, ProvisionErrorKind.Validation));
            Become(Ready);
        }
    }

    private void WaitForSlugCheck(
        ProvisionTenantRequest request,
        string normalizedSlug,
        CorrelationFlow flow,
        IActorRef originalSender)
    {
        Receive<GetTenantBySlugResult>(result =>
        {
            using CorrelationScope scope = flow.BeginScope();
            if (result.Tenant is not null)
            {
                originalSender.Tell(new ProvisionTenantResult(
                    false,
                    null,
                    $"Tenant slug '{normalizedSlug}' already exists.",
                    ProvisionErrorKind.Conflict));
                Become(Ready);
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var tenant = new TenantRecord(
                TenantId: Guid.NewGuid(),
                Slug: normalizedSlug,
                DisplayName: request.DisplayName.Trim(),
                Status: TenantLifecycleStatus.Provisioning,
                DeploymentProfile: TenantRecordMapper.BuildDeploymentProfile(request),
                PackEntitlements: new TenantPackEntitlements(
                    request.IntegrationPacks?.ToArray() ?? [],
                    request.CustomizationPacks?.ToArray() ?? []),
                Migration: new TenantMigrationState(TenantMigrationPhase.None, null, null, null),
                Billing: new TenantBillingStub(request.BillingTier, request.SeatLimit),
                CreatedAt: now,
                ProvisionedAt: null,
                LastSyncedToPlatformAt: null,
                LastPlatformSyncError: null);

            _persistActor.Tell(flow.WrapChild(new AddTenantCommand(tenant), "Tenant.Add"), Self);
            Become(() => WaitForAddThenSync(tenant, flow, originalSender));
        });
    }

    private void WaitForAddThenSync(
        TenantRecord tenant,
        CorrelationFlow flow,
        IActorRef originalSender)
    {
        Receive<AddTenantResult>(result =>
        {
            using CorrelationScope scope = flow.BeginScope();
            if (!result.Success || result.Tenant is null)
            {
                originalSender.Tell(new ProvisionTenantResult(
                    false,
                    null,
                    result.ErrorMessage ?? "Failed to persist tenant.",
                    ProvisionErrorKind.Validation));
                Become(Ready);
                return;
            }

            _log.Info("Created tenant {0} ({1}) in control plane", result.Tenant.TenantId, result.Tenant.Slug);
            _platformSyncActor.Tell(
                flow.WrapChild(new PushTenantConfigCommand(result.Tenant), "Tenant.PushConfig"),
                Self);
            Become(() => WaitForProvisionSync(result.Tenant, flow, originalSender));
        });
    }

    private void WaitForProvisionSync(
        TenantRecord tenant,
        CorrelationFlow flow,
        IActorRef originalSender)
    {
        Receive<PushTenantConfigResult>(syncResult =>
        {
            using CorrelationScope scope = flow.BeginScope();
            var syncedAt = DateTimeOffset.UtcNow;
            var updated = TenantRecordMapper.WithSyncResult(
                tenant,
                success: syncResult.Success,
                errorMessage: syncResult.ErrorMessage,
                syncedAt);

            _persistActor.Tell(flow.WrapChild(new UpdateTenantCommand(updated), "Tenant.UpdateSync"), Self);
            Become(() => WaitForProvisionUpdate(updated, syncResult, flow, originalSender));
        });
    }

    private void WaitForProvisionUpdate(
        TenantRecord updated,
        PushTenantConfigResult syncResult,
        CorrelationFlow flow,
        IActorRef originalSender)
    {
        Receive<UpdateTenantResult>(updateResult =>
        {
            using CorrelationScope scope = flow.BeginScope();
            var tenant = updateResult.Tenant ?? updated;

            if (!syncResult.Success)
            {
                originalSender.Tell(new ProvisionTenantResult(
                    false,
                    tenant,
                    syncResult.ErrorMessage ?? "Platform sync failed.",
                    ProvisionErrorKind.PlatformSync));
                Become(Ready);
                return;
            }

            originalSender.Tell(new ProvisionTenantResult(true, tenant, null, null));
            Become(Ready);
        });
    }

    private void StartSync(SyncTenantCommand command, CorrelationFlow flow, IActorRef sender)
    {
        _persistActor.Tell(flow.WrapChild(new GetTenantByIdQuery(command.TenantId), "Tenant.GetById"), Self);
        Become(() => WaitForSyncLookup(command.TenantId, flow, sender));
    }

    private void WaitForSyncLookup(Guid tenantId, CorrelationFlow flow, IActorRef originalSender)
    {
        Receive<GetTenantByIdResult>(lookup =>
        {
            using CorrelationScope scope = flow.BeginScope();
            if (lookup.Tenant is null)
            {
                originalSender.Tell(new SyncTenantResult(false, null, $"Tenant '{tenantId}' was not found."));
                Become(Ready);
                return;
            }

            _platformSyncActor.Tell(
                flow.WrapChild(new PushTenantConfigCommand(lookup.Tenant), "Tenant.PushConfig"),
                Self);
            Become(() => WaitForSyncPush(lookup.Tenant, flow, originalSender));
        });
    }

    private void WaitForSyncPush(
        TenantRecord tenant,
        CorrelationFlow flow,
        IActorRef originalSender)
    {
        Receive<PushTenantConfigResult>(syncResult =>
        {
            using CorrelationScope scope = flow.BeginScope();
            var syncedAt = DateTimeOffset.UtcNow;
            var updated = TenantRecordMapper.WithSyncResult(
                tenant,
                success: syncResult.Success,
                errorMessage: syncResult.ErrorMessage,
                syncedAt);

            _persistActor.Tell(flow.WrapChild(new UpdateTenantCommand(updated), "Tenant.UpdateSync"), Self);
            Become(() => WaitForSyncUpdate(updated, syncResult, flow, originalSender));
        });
    }

    private void WaitForSyncUpdate(
        TenantRecord updated,
        PushTenantConfigResult syncResult,
        CorrelationFlow flow,
        IActorRef originalSender)
    {
        Receive<UpdateTenantResult>(updateResult =>
        {
            using CorrelationScope scope = flow.BeginScope();
            var tenant = updateResult.Tenant ?? updated;

            if (!syncResult.Success)
            {
                originalSender.Tell(new SyncTenantResult(false, tenant, syncResult.ErrorMessage));
                Become(Ready);
                return;
            }

            _log.Info("Synced tenant {0} ({1}) to platform", tenant.TenantId, tenant.Slug);
            originalSender.Tell(new SyncTenantResult(true, tenant, null));
            Become(Ready);
        });
    }

    private static void ValidateRequest(ProvisionTenantRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            throw new ArgumentException("Slug is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new ArgumentException("DisplayName is required.", nameof(request));
        }
    }
}
