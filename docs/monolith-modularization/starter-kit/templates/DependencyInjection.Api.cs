// <Context>.Api/DependencyInjection.cs — module facade for the host.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using <Context>.Application;
using <Context>.Infrastructure;

namespace <Context>.Api;

public static class DependencyInjection
{
    public static IServiceCollection Add<Context>Module(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Add<Context>Application();
        services.Add<Context>Infrastructure(configuration);
        return services;
    }

    public static WebApplication Map<Context>Module(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.Map<Context>Endpoints();
        return app;
    }
}
