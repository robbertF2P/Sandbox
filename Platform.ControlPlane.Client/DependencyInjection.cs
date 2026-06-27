using Microsoft.Extensions.DependencyInjection;

namespace Platform.ControlPlane.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformConfigurationClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpClient<IPlatformConfigurationClient, PlatformConfigurationHttpClient>();
        return services;
    }
}
