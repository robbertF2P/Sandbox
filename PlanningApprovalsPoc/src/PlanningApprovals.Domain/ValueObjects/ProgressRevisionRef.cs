using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.ValueObjects;

public sealed record ProgressRevisionRef(
    long AssignmentId,
    long RevisionId,
    DateTimeOffset RecordedAt,
    decimal PercentComplete,
    decimal BookedHours,
    string Source)
{
    public string Fingerprint { get; } = ContentFingerprint.FromParts(
        AssignmentId.ToString(),
        RevisionId.ToString(),
        PercentComplete.ToString("0.####"),
        BookedHours.ToString("0.####"),
        Source);
}
