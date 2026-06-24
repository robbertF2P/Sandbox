namespace PlanningApprovals.Domain.ValueObjects;

/// <summary>
/// Progress and proposed plan captured at a single point in time.
/// </summary>
public sealed record PlanningStateSnapshot(
    DateTimeOffset CapturedAt,
    ProgressRevisionRef ProgressRevision,
    PlanSnapshot PlanSnapshot);
