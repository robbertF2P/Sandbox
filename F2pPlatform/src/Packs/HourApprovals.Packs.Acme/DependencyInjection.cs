using HourApprovals.Application.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace HourApprovals.Packs.Acme;

public sealed class AcmeHourApprovalsPack : IHourApprovalsCustomizationPack
{
    public string PackId => "acme-hour-approvals-v1";

    public HourApprovalsDisplaySettings DisplaySettings =>
        new(ShowPlannedStart: true, ShowPlannedFinish: true);
}

public static class DependencyInjection
{
    public static IServiceCollection AddAcmeHourApprovalsPack(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IHourApprovalsCustomizationPack, AcmeHourApprovalsPack>();
        return services;
    }
}
