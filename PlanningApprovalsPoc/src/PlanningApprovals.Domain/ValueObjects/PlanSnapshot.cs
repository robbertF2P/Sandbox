namespace PlanningApprovals.Domain.ValueObjects;

public sealed record PlanSnapshot(
    DateOnly PlannedStart,
    DateOnly PlannedFinish,
    decimal PlannedHours,
    ProfileFingerprint ProfileFingerprint,
    CalculationRunId CalculationRunId)
{
    public string Fingerprint { get; } = ContentFingerprint.FromParts(
        PlannedStart.ToString("O"),
        PlannedFinish.ToString("O"),
        PlannedHours.ToString("0.####"),
        ProfileFingerprint.Value,
        CalculationRunId.Value);
}
