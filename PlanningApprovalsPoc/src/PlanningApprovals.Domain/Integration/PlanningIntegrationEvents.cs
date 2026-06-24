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
