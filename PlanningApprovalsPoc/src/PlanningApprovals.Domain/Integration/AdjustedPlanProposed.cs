using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Integration;

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
