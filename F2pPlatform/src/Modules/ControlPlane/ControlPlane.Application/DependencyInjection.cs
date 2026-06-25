using ControlPlane.Application.Tenants;
using Microsoft.Extensions.DependencyInjection;

namespace ControlPlane.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddControlPlaneApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<TenantProvisioningService>();
        return services;
    }
}
