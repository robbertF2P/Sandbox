using Microsoft.Extensions.DependencyInjection;
using PlatformConfig.Application.Ports;
using PlatformConfig.Application.Services;

namespace PlatformConfig.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformConfigApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<ITenantConfigurationService, TenantConfigurationService>();
        return services;
    }
}
