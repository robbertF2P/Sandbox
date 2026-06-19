namespace ApiImportActorPoc.Contracts.Platform;

public sealed record TenantPackEntitlements(
    IReadOnlyList<string> IntegrationPacks,
    IReadOnlyList<string> CustomizationPacks);
