using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.ControlPlane.Contracts;
using PlatformConfig.Application;
using PlatformConfig.Application.Services;
using PlatformConfig.Infrastructure;

namespace PlatformConfig.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformConfigModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<PlatformConfigurationApiOptions>(
            configuration.GetSection(PlatformConfigurationApiOptions.SectionName));
        services.AddPlatformConfigApplication();
        services.AddPlatformConfigInfrastructure();
        return services;
    }

    public static WebApplication MapPlatformConfigModule(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.MapPlatformConfigEndpoints();
        return app;
    }
}

internal static class PlatformConfigEndpoints
{
    public static IEndpointRouteBuilder MapPlatformConfigEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/platform")
            .WithTags("Platform");

        group.MapPut("/tenant-config", async (
                TenantConfigurationPayload configuration,
                HttpContext httpContext,
                ITenantConfigurationService configurationService,
                CancellationToken cancellationToken) =>
            {
                if (!IsAuthorized(httpContext))
                {
                    return Results.Unauthorized();
                }

                try
                {
                    TenantConfigurationPayload registered = await configurationService.RegisterAsync(
                        configuration,
                        cancellationToken);
                    return Results.Ok(registered);
                }
                catch (ArgumentException exception)
                {
                    return Results.BadRequest(new { error = exception.Message });
                }
            })
            .WithName("RegisterTenantConfiguration")
            .WithSummary("Receive tenant configuration pushed from admin backoffice.");

        group.MapGet("/tenants/{slug}", async (
                string slug,
                ITenantConfigurationService configurationService,
                CancellationToken cancellationToken) =>
            {
                TenantConfigurationPayload? tenant = await configurationService.ResolveBySlugAsync(
                    slug,
                    cancellationToken);
                return tenant is null ? Results.NotFound() : Results.Ok(tenant);
            })
            .WithName("ResolveTenantBySlug")
            .WithSummary("Resolve tenant configuration by slug (edge resolver stub).");

        group.MapGet("/tenants", async (
                ITenantConfigurationService configurationService,
                CancellationToken cancellationToken) =>
            {
                IReadOnlyList<TenantConfigurationPayload> tenants =
                    await configurationService.ListAsync(cancellationToken);
                return Results.Ok(tenants);
            })
            .WithName("ListRegisteredTenants")
            .WithSummary("List tenant configurations registered on this runtime.");

        return app;
    }

    private static bool IsAuthorized(HttpContext httpContext)
    {
        var options = httpContext.RequestServices
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<PlatformConfigurationApiOptions>>()
            .Value;

        if (string.IsNullOrWhiteSpace(options.ConfigurationApiKey))
        {
            return false;
        }

        if (!httpContext.Request.Headers.TryGetValue("X-Platform-Config-Key", out var providedKey))
        {
            return false;
        }

        return string.Equals(providedKey.ToString(), options.ConfigurationApiKey, StringComparison.Ordinal);
    }
}

public sealed class PlatformConfigurationApiOptions
{
    public const string SectionName = "Platform";

    public string ConfigurationApiKey { get; set; } = "dev-platform-config-key";
}
