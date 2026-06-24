using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Services;

public static class LookbackBaselineResolver
{
    /// <summary>
    /// Picks the latest checkpoint at or before the lookback cutoff (~1 week ago).
    /// </summary>
    public static PlanningStateSnapshot? Resolve(
        IEnumerable<AssignmentPlanningCheckpoint> checkpoints,
        DateTimeOffset asOf,
        ApprovalLookbackWindow window)
    {
        DateTimeOffset cutoff = window.BaselineCutoff(asOf);

        AssignmentPlanningCheckpoint? match = checkpoints
            .Where(checkpoint => checkpoint.CapturedAt <= cutoff)
            .MaxBy(checkpoint => checkpoint.CapturedAt);

        return match?.ToStateSnapshot();
    }
}
