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
