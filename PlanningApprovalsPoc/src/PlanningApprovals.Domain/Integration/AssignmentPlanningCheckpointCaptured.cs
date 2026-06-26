using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Integration;

/// <summary>
/// Published by Planning when assignment progress and plan are captured for lookback (nightly or on event).
/// </summary>
public sealed record AssignmentPlanningCheckpointCaptured(
    AssignmentId AssignmentId,
    DateTimeOffset CapturedAt,
    ProgressRevisionRef ProgressRevision,
    PlanSnapshot PlanSnapshot,
    CaptureSource CaptureSource);
