namespace ControlPlane.Domain.Tenants;

public enum TenantLifecycleStatus
{
    Provisioning,
    Active,
    Suspended,
    Migrating,
    Retired
}
