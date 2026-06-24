using HourApprovals.Application.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HourApprovals.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHourApprovalsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<IHourApprovalsFeatureGate, ConfigurationHourApprovalsFeatureGate>();
        services.AddSingleton<IHourApprovalsRepository, InMemoryHourApprovalsRepository>();

        if (!services.Any(descriptor => descriptor.ServiceType == typeof(IHourApprovalsCustomizationPack)))
        {
            services.AddSingleton<IHourApprovalsCustomizationPack, DefaultHourApprovalsPack>();
        }

        return services;
    }
}
