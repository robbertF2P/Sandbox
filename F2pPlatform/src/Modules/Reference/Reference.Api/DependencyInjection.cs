using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reference.Application;
using Reference.Application.Ports;
using Reference.Domain;
using Reference.Infrastructure;

namespace Reference.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddReferenceModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var useLegacyAdapter = configuration.GetValue("Reference:UseLegacyAdapter", false);
        services.AddReferenceApplication();
        services.AddReferenceInfrastructure(useLegacyAdapter);
        return services;
    }

    public static WebApplication MapReferenceModule(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.MapReferenceEndpoints();
        return app;
    }
}

internal static class ReferenceEndpoints
{
    public static IEndpointRouteBuilder MapReferenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reference")
            .WithTags("Reference");

        group.MapGet("/status", async (
                IReferenceStatusQuery query,
                CancellationToken cancellationToken) =>
            {
                ReferenceStatusSnapshot snapshot = await query.GetStatusAsync(cancellationToken);
                ReferenceHealth health = ReferenceStatusRules.ResolveHealth(
                    snapshot.ModuleRegistered,
                    snapshot.StranglerAdapterPresent);

                return Results.Ok(new
                {
                    snapshot.ModuleName,
                    Health = health.ToString(),
                    snapshot.ModuleRegistered,
                    snapshot.StranglerAdapterPresent,
                    snapshot.CheckedAtUtc,
                });
            })
            .WithName("GetReferenceStatus")
            .WithSummary("Smoke endpoint proving module registration and DI wiring.");

        return app;
    }
}
