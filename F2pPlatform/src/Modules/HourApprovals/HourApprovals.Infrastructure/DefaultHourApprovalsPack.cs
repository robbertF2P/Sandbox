using HourApprovals.Application.Ports;

namespace HourApprovals.Infrastructure;

public sealed class DefaultHourApprovalsPack : IHourApprovalsCustomizationPack
{
    public string PackId => "default-hour-approvals-v1";

    public HourApprovalsDisplaySettings DisplaySettings =>
        new(ShowPlannedStart: false, ShowPlannedFinish: false);
}
