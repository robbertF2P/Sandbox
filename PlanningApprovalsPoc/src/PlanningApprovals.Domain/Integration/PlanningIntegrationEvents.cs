using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Integration;

/// <summary>
/// Published by Planning when assignment progress history is appended.
/// </summary>
public sealed record AssignmentProgressRevisionRecorded(
    long ProjectId,
    long AssignmentId,
    ProgressRevisionRef ProgressRevision,
    DateTimeOffset OccurredAt);

/// <summary>
/// Published by Planning when schedule recalculation produces a new proposed plan.
/// </summary>
public sealed record AdjustedPlanProposed(
    long ProjectId,
    long AssignmentId,
    ProgressRevisionRef ProgressRevision,
    PlanSnapshot ProposedPlan,
    DateTimeOffset OccurredAt,
    string CalculationRunId);

/// <summary>
/// Published by Planning when assignment progress and plan are captured for lookback (nightly or on event).
/// </summary>
public sealed record AssignmentPlanningCheckpointCaptured(
    long AssignmentId,
    DateTimeOffset CapturedAt,
    ProgressRevisionRef ProgressRevision,
    PlanSnapshot PlanSnapshot,
    string CaptureSource);
