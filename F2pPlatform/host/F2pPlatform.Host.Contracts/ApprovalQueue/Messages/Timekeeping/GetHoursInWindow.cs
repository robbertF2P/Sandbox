namespace F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Timekeeping;

public sealed record GetHoursInWindow(
    IReadOnlyList<Guid> AssignmentIds,
    string TimeRangePreset);

public sealed record GetHoursInWindowReply(
    IReadOnlyDictionary<Guid, decimal> HoursByAssignmentId);
