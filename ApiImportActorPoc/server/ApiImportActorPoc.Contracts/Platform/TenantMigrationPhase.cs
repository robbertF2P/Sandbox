namespace ApiImportActorPoc.Contracts.Platform;

public enum TenantMigrationPhase
{
    None,
    Exporting,
    DryRun,
    Importing,
    Cutover,
    RolledBack
}
