using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Integration;

/// <summary>
/// Published by Planning when assignment progress history is appended.
/// </summary>
public sealed record AssignmentProgressRevisionRecorded(
    ProjectId ProjectId,
    AssignmentId AssignmentId,
    ProgressRevisionRef ProgressRevision,
    DateTimeOffset OccurredAt);

/// <summary>
/// Published by Planning when schedule recalculation produces a new proposed plan.
/// </summary>
public sealed record AdjustedPlanProposed(
    ProjectId ProjectId,
    AssignmentId AssignmentId,
    ProgressRevisionRef ProgressRevision,
    PlanSnapshot ProposedPlan,
    DateTimeOffset OccurredAt,
    CalculationRunId CalculationRunId);

/// <summary>
/// Published by Planning when assignment progress and plan are captured for lookback (nightly or on event).
/// </summary>
public sealed record AssignmentPlanningCheckpointCaptured(
    AssignmentId AssignmentId,
    DateTimeOffset CapturedAt,
    ProgressRevisionRef ProgressRevision,
    PlanSnapshot PlanSnapshot,
    CaptureSource CaptureSource);
