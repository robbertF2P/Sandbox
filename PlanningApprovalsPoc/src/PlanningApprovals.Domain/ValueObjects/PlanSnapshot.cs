using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.ValueObjects;

public sealed record PlanSnapshot(
    DateOnly PlannedStart,
    DateOnly PlannedFinish,
    decimal PlannedHours,
    string ProfileFingerprint,
    string CalculationRunId)
{
    public string Fingerprint { get; } = ContentFingerprint.FromParts(
        PlannedStart.ToString("O"),
        PlannedFinish.ToString("O"),
        PlannedHours.ToString("0.####"),
        ProfileFingerprint,
        CalculationRunId);
}
