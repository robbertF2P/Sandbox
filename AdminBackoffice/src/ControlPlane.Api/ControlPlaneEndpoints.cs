using ControlPlane.Application.Ports;
using ControlPlane.Contracts.Interfaces;
using ControlPlane.Contracts.Messages.Provisioning;
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
                IControlPlaneActorFacade actorFacade,
                CancellationToken cancellationToken) =>
            {
                ProvisionTenantResult result = await actorFacade.ProvisionTenantAsync(
                    body.ToRequest(),
                    cancellationToken);

                if (result.Success && result.Tenant is not null)
                {
                    return Results.Created($"/admin/tenants/{result.Tenant.TenantId}", result.Tenant);
                }

                return result.ErrorKind switch
                {
                    ProvisionErrorKind.Validation => Results.BadRequest(new { error = result.ErrorMessage }),
                    ProvisionErrorKind.Conflict => Results.Conflict(new { error = result.ErrorMessage }),
                    ProvisionErrorKind.PlatformSync => PlatformSyncProblem(result),
                    _ => Results.Problem(detail: result.ErrorMessage ?? "Provisioning failed.")
                };
            })
            .WithName("ProvisionTenant")
            .WithSummary("Provision a tenant via Akka actor pipeline, persist, and push config to the v2 platform.");

        group.MapPost("/tenants/{tenantId:guid}/sync", async (
                Guid tenantId,
                IControlPlaneActorFacade actorFacade,
                CancellationToken cancellationToken) =>
            {
                SyncTenantResult result = await actorFacade.SyncTenantAsync(tenantId, cancellationToken);

                if (result.Success && result.Tenant is not null)
                {
                    return Results.Ok(result.Tenant);
                }

                if (result.ErrorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Results.NotFound();
                }

                return PlatformSyncProblem(result.ErrorMessage, result.Tenant?.Slug);
            })
            .WithName("SyncTenantToPlatform")
            .WithSummary("Re-push tenant configuration to the v2 platform runtime via Akka actors.");

        return app;
    }

    private static IResult PlatformSyncProblem(ProvisionTenantResult result) =>
        PlatformSyncProblem(result.ErrorMessage, result.Tenant?.Slug);

    private static IResult PlatformSyncProblem(string? errorMessage, string? tenantSlug = null)
    {
        var detail = BuildPlatformSyncDetail(errorMessage, tenantSlug);
        return Results.Problem(
            detail: detail,
            statusCode: StatusCodes.Status502BadGateway,
            title: "Platform sync failed",
            extensions: new Dictionary<string, object?> { ["error"] = detail });
    }

    private static string BuildPlatformSyncDetail(string? errorMessage, string? tenantSlug)
    {
        var root = string.IsNullOrWhiteSpace(errorMessage) ? "Platform sync failed." : errorMessage.Trim();

        if (string.IsNullOrWhiteSpace(tenantSlug))
        {
            return root;
        }

        return $"{root} Tenant '{tenantSlug}' is saved in the control plane — start F2pPlatform on :5080 and use Sync.";
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
