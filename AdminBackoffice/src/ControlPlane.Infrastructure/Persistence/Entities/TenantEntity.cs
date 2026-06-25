using Platform.ControlPlane.Contracts;

namespace ControlPlane.Infrastructure.Persistence.Entities;

public sealed class TenantEntity
{
    public Guid TenantId { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public TenantLifecycleStatus Status { get; set; }

    public TenantDeploymentMode Mode { get; set; }

    public TenantDataTier DataTier { get; set; }

    public string Region { get; set; } = string.Empty;

    public string? LegacyBuildProfileId { get; set; }

    public string? LegacyRuntimeUrl { get; set; }

    public string? LegacyDatabaseConnectionRef { get; set; }

    public string? NativeDatabaseConnectionRef { get; set; }

    public string? NativeApiBaseUrl { get; set; }

    public string IntegrationPacksJson { get; set; } = "[]";

    public string CustomizationPacksJson { get; set; } = "[]";

    public TenantMigrationPhase MigrationPhase { get; set; }

    public TenantDeploymentMode? MigrationTargetMode { get; set; }

    public DateTimeOffset? LastExportAt { get; set; }

    public DateTimeOffset? CutoverAt { get; set; }

    public string BillingTier { get; set; } = "standard";

    public int SeatLimit { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ProvisionedAt { get; set; }

    public DateTimeOffset? LastSyncedToPlatformAt { get; set; }

    public string? LastPlatformSyncError { get; set; }
}
