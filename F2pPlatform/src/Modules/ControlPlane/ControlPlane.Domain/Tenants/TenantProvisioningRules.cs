using System.Text.RegularExpressions;

namespace ControlPlane.Domain.Tenants;

public static partial class TenantProvisioningRules
{
    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    public static bool IsValidSlug(string? slug) =>
        !string.IsNullOrWhiteSpace(slug) && SlugPattern().IsMatch(slug);

    public static bool IsValidDisplayName(string? displayName) =>
        !string.IsNullOrWhiteSpace(displayName) && displayName.Trim().Length <= 200;

    public static bool IsSlugAvailable(string slug, IReadOnlyCollection<TenantRecord> existingTenants) =>
        existingTenants.All(tenant => !string.Equals(tenant.Slug, slug, StringComparison.Ordinal));

    public static TenantRecord CreateNativeTenant(
        string slug,
        string displayName,
        TenantDataTier dataTier,
        string region,
        string databaseConnectionRef,
        string apiBaseUrl,
        string billingTier,
        int seatLimit,
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(region);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseConnectionRef);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiBaseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(billingTier);

        if (!IsValidSlug(slug))
        {
            throw new ArgumentException("Slug must be lowercase alphanumeric with optional hyphens.", nameof(slug));
        }

        if (!IsValidDisplayName(displayName))
        {
            throw new ArgumentException("Display name is required and must be 200 characters or fewer.", nameof(displayName));
        }

        if (seatLimit < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(seatLimit), "Seat limit must be at least 1.");
        }

        return new TenantRecord(
            Guid.NewGuid(),
            slug.Trim(),
            displayName.Trim(),
            TenantLifecycleStatus.Provisioning,
            TenantDeploymentProfile.CreateNative(
                dataTier,
                region.Trim(),
                new NativeRuntimeProfile(databaseConnectionRef.Trim(), apiBaseUrl.Trim())),
            new TenantPackEntitlements([], []),
            new TenantMigrationState(TenantMigrationPhase.None, null, null, null),
            new TenantBillingStub(billingTier.Trim(), seatLimit),
            createdAtUtc);
    }
}
