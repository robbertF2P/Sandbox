using HourApprovals.Application.Ports;
using Microsoft.Extensions.Configuration;

namespace HourApprovals.Infrastructure;

internal sealed class ConfigurationHourApprovalsFeatureGate(IConfiguration configuration)
    : IHourApprovalsFeatureGate
{
    public bool IsEnabled =>
        configuration.GetValue("Tenant:FeatureFlags:hours-progress-approval", false);
}
