namespace ControlPlane.Domain.Tenants;

public sealed record TenantRecord(
    Guid TenantId,
    string Slug,
    string DisplayName,
    TenantLifecycleStatus Status,
    TenantDeploymentProfile DeploymentProfile,
    TenantPackEntitlements PackEntitlements,
    TenantMigrationState Migration,
    TenantBillingStub Billing,
    DateTimeOffset CreatedAtUtc);
