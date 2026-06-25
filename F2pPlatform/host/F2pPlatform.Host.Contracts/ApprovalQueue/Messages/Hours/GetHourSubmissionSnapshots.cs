using F2pPlatform.Host.Contracts.ApprovalQueue;

namespace F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Hours;

public sealed record HourSubmissionSnapshot(
    TaskId TaskId,
    ApprovalState ApprovalState,
    ApprovalProgressValues CurrentValues,
    LastSubmission? LastSubmission);

public sealed record GetHourSubmissionSnapshots(IReadOnlyList<TaskId> TaskIds);

public sealed record GetHourSubmissionSnapshotsReply(
    IReadOnlyDictionary<TaskId, HourSubmissionSnapshot> SnapshotsByTaskId);
