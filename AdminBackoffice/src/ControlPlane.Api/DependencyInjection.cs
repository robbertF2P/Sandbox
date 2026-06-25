using ControlPlane.Api.Services;
using ControlPlane.Application;
using ControlPlane.Infrastructure;
using Microsoft.AspNetCore.Builder;
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
        services.AddControlPlaneInfrastructure(configuration);
        services.AddControlPlaneActors();
        return services;
    }

    public static WebApplication MapControlPlaneModule(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.MapControlPlaneEndpoints();
        return app;
    }
}
