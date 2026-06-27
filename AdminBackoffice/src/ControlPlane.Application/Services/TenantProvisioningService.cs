using ControlPlane.Application.Ports;
using Microsoft.Extensions.Logging;
using Platform.ControlPlane.Client;
using Platform.ControlPlane.Contracts;

namespace ControlPlane.Application.Services;

public sealed class TenantProvisioningService(
    ITenantRepository tenantRepository,
    IPlatformConfigurationClient platformConfigurationClient,
    ILogger<TenantProvisioningService> logger) : ITenantProvisioningService
{
    public async Task<TenantRecord> ProvisionAsync(
        ProvisionTenantRequest request,
        CancellationToken cancellationToken)
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

        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        if (await tenantRepository.GetBySlugAsync(normalizedSlug, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"Tenant slug '{normalizedSlug}' already exists.");
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

        await tenantRepository.AddAsync(tenant, cancellationToken);
        logger.LogInformation("Created tenant {TenantId} ({Slug}) in control plane", tenant.TenantId, tenant.Slug);

        return await SyncToPlatformAsync(tenant.TenantId, cancellationToken);
    }

    public async Task<TenantRecord> SyncToPlatformAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(tenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant '{tenantId}' was not found.");

        var syncedAt = DateTimeOffset.UtcNow;

        try
        {
            await platformConfigurationClient.PushTenantConfigurationAsync(
                TenantRecordMapper.ToConfigurationPayload(tenant),
                cancellationToken);

            var updated = TenantRecordMapper.WithSyncResult(tenant, success: true, errorMessage: null, syncedAt);
            await tenantRepository.UpdateAsync(updated, cancellationToken);
            logger.LogInformation("Synced tenant {TenantId} ({Slug}) to platform", updated.TenantId, updated.Slug);
            return updated;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to sync tenant {TenantId} ({Slug}) to platform", tenant.TenantId, tenant.Slug);
            var failed = TenantRecordMapper.WithSyncResult(
                tenant,
                success: false,
                errorMessage: exception.Message,
                syncedAt);
            await tenantRepository.UpdateAsync(failed, cancellationToken);
            throw;
        }
    }
}
