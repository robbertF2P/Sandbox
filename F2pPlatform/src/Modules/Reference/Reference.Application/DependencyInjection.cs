using Microsoft.Extensions.DependencyInjection;

namespace Reference.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddReferenceApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}
