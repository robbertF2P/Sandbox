using ControlPlane.Application.Ports;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Platform.ControlPlane.Contracts;

namespace ControlPlane.Api;

internal static class ControlPlaneEndpoints
{
    public static IEndpointRouteBuilder MapControlPlaneEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin")
            .WithTags("Admin");

        group.MapGet("/tenants", async (
                ITenantRepository tenantRepository,
                CancellationToken cancellationToken) =>
            {
                IReadOnlyList<TenantRecord> tenants = await tenantRepository.ListAsync(cancellationToken);
                return Results.Ok(tenants);
            })
            .WithName("ListTenants")
            .WithSummary("List all tenants in the control-plane registry.");

        group.MapGet("/tenants/{tenantId:guid}", async (
                Guid tenantId,
                ITenantRepository tenantRepository,
                CancellationToken cancellationToken) =>
            {
                TenantRecord? tenant = await tenantRepository.GetByIdAsync(tenantId, cancellationToken);
                return tenant is null ? Results.NotFound() : Results.Ok(tenant);
            })
            .WithName("GetTenant")
            .WithSummary("Get a tenant by id.");

        group.MapPost("/tenants", async (
                ProvisionTenantRequestBody body,
                ITenantProvisioningService provisioningService,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    TenantRecord tenant = await provisioningService.ProvisionAsync(
                        body.ToRequest(),
                        cancellationToken);
                    return Results.Created($"/admin/tenants/{tenant.TenantId}", tenant);
                }
                catch (ArgumentException exception)
                {
                    return Results.BadRequest(new { error = exception.Message });
                }
                catch (InvalidOperationException exception)
                {
                    return Results.Conflict(new { error = exception.Message });
                }
            })
            .WithName("ProvisionTenant")
            .WithSummary("Provision a tenant, persist to control-plane DB, and push config to the v2 platform.");

        group.MapPost("/tenants/{tenantId:guid}/sync", async (
                Guid tenantId,
                ITenantProvisioningService provisioningService,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    TenantRecord tenant = await provisioningService.SyncToPlatformAsync(tenantId, cancellationToken);
                    return Results.Ok(tenant);
                }
                catch (KeyNotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("SyncTenantToPlatform")
            .WithSummary("Re-push tenant configuration to the v2 platform runtime.");

        return app;
    }

    private sealed record ProvisionTenantRequestBody(
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
        int SeatLimit = 50)
    {
        public ProvisionTenantRequest ToRequest() =>
            new(
                Slug,
                DisplayName,
                Mode,
                DataTier,
                Region,
                LegacyBuildProfileId,
                LegacyRuntimeUrl,
                LegacyDatabaseConnectionRef,
                NativeDatabaseConnectionRef,
                NativeApiBaseUrl,
                IntegrationPacks,
                CustomizationPacks,
                BillingTier,
                SeatLimit);
    }
}
