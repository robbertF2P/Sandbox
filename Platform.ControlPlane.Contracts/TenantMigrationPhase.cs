namespace Platform.ControlPlane.Contracts;

public enum TenantMigrationPhase
{
    None,
    Exporting,
    DryRun,
    Importing,
    Cutover,
    RolledBack
}
