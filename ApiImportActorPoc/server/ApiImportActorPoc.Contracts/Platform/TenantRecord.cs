namespace ApiImportActorPoc.Contracts.Platform;

/// <summary>
/// Control-plane tenant registry entry. Product data lives in legacy or native runtime DBs.
/// </summary>
public sealed record TenantRecord(
    Guid TenantId,
    string Slug,
    string DisplayName,
    TenantLifecycleStatus Status,
    TenantDeploymentProfile DeploymentProfile,
    TenantPackEntitlements PackEntitlements,
    TenantMigrationState Migration,
    TenantBillingStub Billing);
