namespace ControlPlane.Domain.Tenants;

public enum TenantMigrationPhase
{
    None,
    Exporting,
    Validating,
    Cutover,
    Completed
}
