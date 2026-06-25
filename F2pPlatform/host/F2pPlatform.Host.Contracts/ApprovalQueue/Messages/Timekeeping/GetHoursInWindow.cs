using F2pPlatform.Host.Contracts.ApprovalQueue;

namespace F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Timekeeping;

public sealed record GetHoursInWindow(
    IReadOnlyList<AssignmentId> AssignmentIds,
    string TimeRangePreset);

public sealed record GetHoursInWindowReply(
    IReadOnlyDictionary<AssignmentId, decimal> HoursByAssignmentId);
