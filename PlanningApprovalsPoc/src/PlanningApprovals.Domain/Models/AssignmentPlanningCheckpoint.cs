using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Models;

/// <summary>
/// Append-only capture of assignment progress and plan from Planning (nightly or on event).
/// Used to resolve the ~1 week lookback baseline.
/// </summary>
public sealed class AssignmentPlanningCheckpoint
{
    private AssignmentPlanningCheckpoint()
    {
    }

    public AssignmentPlanningCheckpoint(
        Guid publicId,
        long assignmentId,
        DateTimeOffset capturedAt,
        ProgressRevisionRef progressRevision,
        PlanSnapshot planSnapshot,
        string captureSource)
    {
        if (string.IsNullOrWhiteSpace(captureSource))
        {
            throw new ArgumentException("Capture source is required.", nameof(captureSource));
        }

        PublicId = publicId;
        AssignmentId = assignmentId;
        CapturedAt = capturedAt;
        ProgressRevision = progressRevision;
        PlanSnapshot = planSnapshot;
        CaptureSource = captureSource;
    }

    public int Id { get; private set; }

    public Guid PublicId { get; private init; }

    public long AssignmentId { get; private init; }

    public DateTimeOffset CapturedAt { get; private init; }

    public ProgressRevisionRef ProgressRevision { get; private init; } = null!;

    public PlanSnapshot PlanSnapshot { get; private init; } = null!;

    public string CaptureSource { get; private init; } = string.Empty;

    public PlanningStateSnapshot ToStateSnapshot() =>
        new(CapturedAt, ProgressRevision, PlanSnapshot);

    public static AssignmentPlanningCheckpoint Capture(
        long assignmentId,
        DateTimeOffset capturedAt,
        ProgressRevisionRef progressRevision,
        PlanSnapshot planSnapshot,
        string captureSource) =>
        new(Guid.NewGuid(), assignmentId, capturedAt, progressRevision, planSnapshot, captureSource);
}
