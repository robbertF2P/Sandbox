namespace ControlPlane.Domain.Tenants;

public sealed record TenantMigrationState(
    TenantMigrationPhase Phase,
    TenantDeploymentMode? TargetMode,
    DateTimeOffset? LastExportAt,
    DateTimeOffset? CutoverAt);
