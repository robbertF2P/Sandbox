using Microsoft.Extensions.DependencyInjection;
using PlatformConfig.Application.Ports;
using PlatformConfig.Infrastructure;

namespace PlatformConfig.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformConfigInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<ITenantConfigurationStore, InMemoryTenantConfigurationStore>();
        return services;
    }
}
