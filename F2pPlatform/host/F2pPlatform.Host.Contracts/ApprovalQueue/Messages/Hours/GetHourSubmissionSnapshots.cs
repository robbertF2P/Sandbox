namespace F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Hours;

public sealed record HourSubmissionSnapshot(
    Guid TaskId,
    string ApprovalState,
    bool IsApproved,
    decimal HoursToGo,
    decimal Progress,
    decimal WorkedHours,
    string? PlannedStart,
    string? PlannedFinish,
    string? LastSubmittedBy,
    DateTimeOffset? LastSubmittedAtUtc);

public sealed record GetHourSubmissionSnapshots(IReadOnlyList<Guid> TaskIds);

public sealed record GetHourSubmissionSnapshotsReply(
    IReadOnlyDictionary<Guid, HourSubmissionSnapshot> SnapshotsByTaskId);
