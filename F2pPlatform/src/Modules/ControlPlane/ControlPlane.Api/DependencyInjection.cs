using ControlPlane.Application;
using ControlPlane.Application.Tenants;
using ControlPlane.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ControlPlane.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddControlPlaneModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddControlPlaneApplication();
        services.AddControlPlaneInfrastructure();
        return services;
    }

    public static WebApplication MapControlPlaneModule(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.MapControlPlaneEndpoints();
        return app;
    }
}

internal static class ControlPlaneEndpoints
{
    public static IEndpointRouteBuilder MapControlPlaneEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin")
            .WithTags("ControlPlane");

        group.MapGet("/tenants", async (
                TenantProvisioningService service,
                CancellationToken cancellationToken) =>
            {
                IReadOnlyList<TenantSummaryResponse> tenants =
                    await service.ListTenantsAsync(cancellationToken);
                return Results.Ok(tenants);
            })
            .WithName("ListTenants")
            .WithSummary("List all tenants in the control-plane registry.");

        group.MapPost("/tenants", async (
                CreateTenantRequest request,
                TenantProvisioningService service,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    TenantSummaryResponse tenant =
                        await service.CreateTenantAsync(request, cancellationToken);
                    return Results.Created($"/admin/tenants/{tenant.Slug}", tenant);
                }
                catch (TenantSlugConflictException exception)
                {
                    return Results.Conflict(new { error = exception.Message });
                }
                catch (ArgumentException exception)
                {
                    return Results.BadRequest(new { error = exception.Message });
                }
            })
            .WithName("CreateTenant")
            .WithSummary("Provision a new native tenant in the control-plane registry.");

        return app;
    }
}
