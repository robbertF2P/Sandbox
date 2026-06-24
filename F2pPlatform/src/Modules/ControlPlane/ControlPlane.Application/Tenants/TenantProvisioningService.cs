using ControlPlane.Application.Ports;
using ControlPlane.Domain.Tenants;

namespace ControlPlane.Application.Tenants;

public sealed record CreateTenantRequest(
    string Slug,
    string DisplayName,
    string DataTier,
    string Region,
    string DatabaseConnectionRef,
    string? ApiBaseUrl,
    string BillingTier,
    int SeatLimit);

public sealed record TenantSummaryResponse(
    Guid TenantId,
    string Slug,
    string DisplayName,
    string Status,
    string DeploymentMode,
    string DataTier,
    string Region,
    string BillingTier,
    int SeatLimit,
    DateTimeOffset CreatedAtUtc);

public sealed class TenantSlugConflictException(string slug)
    : Exception($"A tenant with slug '{slug}' already exists.");

public sealed class TenantProvisioningService(ITenantRepository repository)
{
    public async Task<IReadOnlyList<TenantSummaryResponse>> ListTenantsAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<TenantRecord> tenants = await repository.ListAsync(cancellationToken);
        return tenants
            .OrderBy(tenant => tenant.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(ToSummary)
            .ToList();
    }

    public async Task<TenantSummaryResponse> CreateTenantAsync(
        CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!TenantProvisioningRules.IsValidSlug(request.Slug))
        {
            throw new ArgumentException("Slug must be lowercase alphanumeric with optional hyphens.", nameof(request));
        }

        if (!TenantProvisioningRules.IsValidDisplayName(request.DisplayName))
        {
            throw new ArgumentException("Display name is required.", nameof(request));
        }

        TenantRecord? existing = await repository.GetBySlugAsync(request.Slug.Trim(), cancellationToken);
        if (existing is not null)
        {
            throw new TenantSlugConflictException(request.Slug.Trim());
        }

        TenantDataTier dataTier = ParseDataTier(request.DataTier);
        string apiBaseUrl = string.IsNullOrWhiteSpace(request.ApiBaseUrl)
            ? "https://api.platform.example/v1"
            : request.ApiBaseUrl.Trim();

        TenantRecord tenant = TenantProvisioningRules.CreateNativeTenant(
            request.Slug,
            request.DisplayName,
            dataTier,
            request.Region,
            request.DatabaseConnectionRef,
            apiBaseUrl,
            request.BillingTier,
            request.SeatLimit,
            DateTimeOffset.UtcNow);

        await repository.AddAsync(tenant, cancellationToken);
        return ToSummary(tenant);
    }

    private static TenantDataTier ParseDataTier(string dataTier) =>
        dataTier.Trim().ToLowerInvariant() switch
        {
            "shared_sql_server" => TenantDataTier.SharedSqlServer,
            "dedicated_sql_server" => TenantDataTier.DedicatedSqlServer,
            _ => throw new ArgumentException(
                "Data tier must be shared_sql_server or dedicated_sql_server.",
                nameof(dataTier)),
        };

    private static TenantSummaryResponse ToSummary(TenantRecord tenant) =>
        new(
            tenant.TenantId,
            tenant.Slug,
            tenant.DisplayName,
            tenant.Status.ToString().ToLowerInvariant(),
            tenant.DeploymentProfile.Mode switch
            {
                TenantDeploymentMode.LegacyHosted => "legacy_hosted",
                TenantDeploymentMode.Native => "native",
                _ => "unknown",
            },
            tenant.DeploymentProfile.DataTier switch
            {
                TenantDataTier.SharedSqlServer => "shared_sql_server",
                TenantDataTier.DedicatedSqlServer => "dedicated_sql_server",
                _ => "unknown",
            },
            tenant.DeploymentProfile.Region,
            tenant.Billing.Tier,
            tenant.Billing.SeatLimit,
            tenant.CreatedAtUtc);
}
