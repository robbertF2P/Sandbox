using ControlPlane.Application;
using ControlPlane.Application.Ports;
using ControlPlane.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ControlPlane.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddControlPlaneInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<ITenantRepository, InMemoryTenantRepository>();
        return services;
    }
}
