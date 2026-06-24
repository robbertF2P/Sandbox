namespace Platform.ControlPlane.Contracts;

public sealed record TenantMigrationState(
    TenantMigrationPhase Phase,
    TenantDeploymentMode? TargetMode,
    DateTimeOffset? LastExportAt,
    DateTimeOffset? CutoverAt);
