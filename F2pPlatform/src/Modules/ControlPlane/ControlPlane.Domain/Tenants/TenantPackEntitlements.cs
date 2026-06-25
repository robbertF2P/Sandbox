namespace ControlPlane.Domain.Tenants;

public sealed record TenantPackEntitlements(
    IReadOnlyList<string> IntegrationPacks,
    IReadOnlyList<string> CustomizationPacks);
