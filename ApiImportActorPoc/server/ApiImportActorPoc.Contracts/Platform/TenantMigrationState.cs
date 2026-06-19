namespace ApiImportActorPoc.Contracts.Platform;

public sealed record TenantMigrationState(
    TenantMigrationPhase Phase,
    TenantDeploymentMode? TargetMode,
    DateTimeOffset? LastExportAt,
    DateTimeOffset? CutoverAt);
