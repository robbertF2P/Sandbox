namespace PlanningApprovals.Domain.ValueObjects;

public sealed record ProgressRevisionRef(
    AssignmentId AssignmentId,
    ProgressRevisionId RevisionId,
    DateTimeOffset RecordedAt,
    decimal PercentComplete,
    decimal BookedHours,
    ProgressSource Source)
{
    public string Fingerprint { get; } = ContentFingerprint.FromParts(
        AssignmentId.Value.ToString(),
        RevisionId.Value.ToString(),
        PercentComplete.ToString("0.####"),
        BookedHours.ToString("0.####"),
        Source.Value);
}
