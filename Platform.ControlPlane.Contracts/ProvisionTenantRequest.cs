namespace Platform.ControlPlane.Contracts;

/// <summary>
/// Operator request to provision a tenant in the control plane.
/// </summary>
public sealed record ProvisionTenantRequest(
    string Slug,
    string DisplayName,
    TenantDeploymentMode Mode,
    TenantDataTier DataTier,
    string Region,
    string? LegacyBuildProfileId,
    string? LegacyRuntimeUrl,
    string? LegacyDatabaseConnectionRef,
    string? NativeDatabaseConnectionRef,
    string? NativeApiBaseUrl,
    IReadOnlyList<string>? IntegrationPacks,
    IReadOnlyList<string>? CustomizationPacks,
    string BillingTier = "standard",
    int SeatLimit = 50);
