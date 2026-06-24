using HourApprovals.Application.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace HourApprovals.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddHourApprovalsApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<IHourApprovalsService, HourApprovalsService>();
        return services;
    }
}
