using F2pPlatform.Host.Contracts.ApprovalQueue;
using Platform.Shared.Domain;

namespace F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Timekeeping;

public sealed record GetHoursInWindow(
    IReadOnlyList<AssignmentId> AssignmentIds,
    TimeRangePreset TimeRangePreset);

public sealed record GetHoursInWindowReply(
    IReadOnlyDictionary<AssignmentId, decimal> HoursByAssignmentId);
