namespace HourApprovals.Application.Ports;

public sealed record HourApprovalsDisplaySettings(
    bool ShowPlannedStart,
    bool ShowPlannedFinish);

public interface IHourApprovalsCustomizationPack
{
    string PackId { get; }

    HourApprovalsDisplaySettings DisplaySettings { get; }
}
