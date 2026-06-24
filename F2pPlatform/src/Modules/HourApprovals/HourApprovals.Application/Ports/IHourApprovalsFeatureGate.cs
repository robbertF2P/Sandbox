namespace HourApprovals.Application.Ports;

public interface IHourApprovalsFeatureGate
{
    bool IsEnabled { get; }
}
