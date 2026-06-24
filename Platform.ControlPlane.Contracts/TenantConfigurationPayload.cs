namespace Platform.ControlPlane.Contracts;

/// <summary>
/// Payload pushed from admin backoffice to the v2 platform runtime after provisioning.
/// </summary>
public sealed record TenantConfigurationPayload(
    Guid TenantId,
    string Slug,
    string DisplayName,
    TenantLifecycleStatus Status,
    TenantDeploymentProfile DeploymentProfile,
    TenantPackEntitlements PackEntitlements,
    TenantMigrationState Migration,
    TenantBillingStub Billing);
