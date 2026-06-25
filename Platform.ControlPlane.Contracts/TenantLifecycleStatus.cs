namespace Platform.ControlPlane.Contracts;

public enum TenantLifecycleStatus
{
    Provisioning,
    Active,
    Suspended,
    Migrating,
    Retired
}
