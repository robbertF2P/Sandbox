using Microsoft.Extensions.DependencyInjection;
using Reference.Application.Ports;
using Reference.Infrastructure.Adapters;
using Reference.Infrastructure.Queries;

namespace Reference.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReferenceInfrastructure(
        this IServiceCollection services,
        bool useLegacyAdapter = false)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (useLegacyAdapter)
        {
            services.AddScoped<IReferenceStatusQuery, LegacyReferenceStatusAdapter>();
        }
        else
        {
            services.AddScoped<IReferenceStatusQuery, ReferenceStatusQuery>();
        }

        return services;
    }
}
