using Platform.ControlPlane.Contracts;

namespace ControlPlane.Application.Services;

public static class TenantRecordMapper
{
    public static TenantConfigurationPayload ToConfigurationPayload(TenantRecord tenant) =>
        new(
            tenant.TenantId,
            tenant.Slug,
            tenant.DisplayName,
            tenant.Status,
            tenant.DeploymentProfile,
            tenant.PackEntitlements,
            tenant.Migration,
            tenant.Billing);

    public static TenantRecord WithSyncResult(
        TenantRecord tenant,
        bool success,
        string? errorMessage,
        DateTimeOffset syncedAt)
    {
        if (success)
        {
            return tenant with
            {
                Status = TenantLifecycleStatus.Active,
                ProvisionedAt = tenant.ProvisionedAt ?? syncedAt,
                LastSyncedToPlatformAt = syncedAt,
                LastPlatformSyncError = null
            };
        }

        return tenant with
        {
            LastSyncedToPlatformAt = syncedAt,
            LastPlatformSyncError = errorMessage
        };
    }

    public static TenantDeploymentProfile BuildDeploymentProfile(ProvisionTenantRequest request)
    {
        return request.Mode switch
        {
            TenantDeploymentMode.LegacyHosted => TenantDeploymentProfile.CreateLegacyHosted(
                request.DataTier,
                request.Region,
                new LegacyHostedRuntimeProfile(
                    Required(request.LegacyBuildProfileId, nameof(request.LegacyBuildProfileId)),
                    Required(request.LegacyRuntimeUrl, nameof(request.LegacyRuntimeUrl)),
                    Required(request.LegacyDatabaseConnectionRef, nameof(request.LegacyDatabaseConnectionRef)))),
            TenantDeploymentMode.Native => TenantDeploymentProfile.CreateNative(
                request.DataTier,
                request.Region,
                new NativeRuntimeProfile(
                    Required(request.NativeDatabaseConnectionRef, nameof(request.NativeDatabaseConnectionRef)),
                    Required(request.NativeApiBaseUrl, nameof(request.NativeApiBaseUrl)))),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request.Mode, "Unknown deployment mode.")
        };
    }

    private static string Required(string? value, string fieldName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{fieldName} is required for the selected deployment mode.", fieldName)
            : value;
}
