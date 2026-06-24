namespace Platform.ControlPlane.Contracts;

public sealed record TenantPackEntitlements(
    IReadOnlyList<string> IntegrationPacks,
    IReadOnlyList<string> CustomizationPacks);
