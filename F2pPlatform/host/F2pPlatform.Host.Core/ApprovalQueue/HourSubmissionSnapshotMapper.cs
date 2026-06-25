using F2pPlatform.Host.Contracts.ApprovalQueue;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Hours;
using HourApprovals.Application.Ports;
using HourApprovals.Domain.Enums;

namespace F2pPlatform.Host.Core.ApprovalQueue;

internal static class HourSubmissionSnapshotMapper
{
    public static HourSubmissionSnapshot ToSnapshot(TaskApprovalView view)
    {
        ApprovalState approvalState = view.State == TaskApprovalState.Approved
            ? ApprovalState.Approved
            : ApprovalState.NotApproved;

        LastSubmission? lastSubmission = view.LastApproval is null
            ? null
            : new LastSubmission(
                view.LastApproval.ApprovedBy,
                view.LastApproval.ApprovedAtUtc,
                ToProgressValues(view.LastApproval.ApprovedValues));

        return new HourSubmissionSnapshot(
            view.Task.Id,
            approvalState,
            ToProgressValues(view.Task.CurrentValues),
            lastSubmission);
    }

    private static ApprovalProgressValues ToProgressValues(HourApprovals.Domain.ValueObjects.ApprovalValues values) =>
        new(
            values.HoursToGo,
            values.Progress,
            values.WorkedHours,
            values.PlannedStart,
            values.PlannedFinish);
}
