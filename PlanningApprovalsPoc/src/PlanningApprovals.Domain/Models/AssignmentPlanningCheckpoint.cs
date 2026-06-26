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
        ApprovalPublicId publicId,
        AssignmentId assignmentId,
        DateTimeOffset capturedAt,
        ProgressRevisionRef progressRevision,
        PlanSnapshot planSnapshot,
        CaptureSource captureSource)
    {
        PublicId = publicId;
        AssignmentId = assignmentId;
        CapturedAt = capturedAt;
        ProgressRevision = progressRevision;
        PlanSnapshot = planSnapshot;
        CaptureSource = captureSource;
    }

    public int Id { get; private set; }

    public ApprovalPublicId PublicId { get; private init; }

    public AssignmentId AssignmentId { get; private init; }

    public DateTimeOffset CapturedAt { get; private init; }

    public ProgressRevisionRef ProgressRevision { get; private init; } = null!;

    public PlanSnapshot PlanSnapshot { get; private init; } = null!;

    public CaptureSource CaptureSource { get; private init; }

    public PlanningStateSnapshot ToStateSnapshot() =>
        new(CapturedAt, ProgressRevision, PlanSnapshot);

    public static AssignmentPlanningCheckpoint Capture(
        AssignmentId assignmentId,
        DateTimeOffset capturedAt,
        ProgressRevisionRef progressRevision,
        PlanSnapshot planSnapshot,
        CaptureSource captureSource) =>
        new(new ApprovalPublicId(Guid.NewGuid()), assignmentId, capturedAt, progressRevision, planSnapshot, captureSource);
}
