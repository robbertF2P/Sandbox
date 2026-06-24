using ControlPlane.Application.Ports;
using ControlPlane.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ControlPlane.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddControlPlaneApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
        return services;
    }
}
