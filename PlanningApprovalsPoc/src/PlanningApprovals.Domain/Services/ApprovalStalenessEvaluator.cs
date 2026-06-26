using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Services;

public static class ApprovalStalenessEvaluator
{
    /// <summary>
    /// Decides whether foreman approval is needed by comparing current state to:
    /// 1) last foreman-approved snapshot (if current already approved → up to date), then
    /// 2) progress and plan from ~1 week ago (lookback baseline).
    /// </summary>
    public static StalenessEvaluation Evaluate(
        ProgressRevisionRef currentProgress,
        PlanSnapshot currentProposedPlan,
        PlanningStateSnapshot? lookbackBaseline,
        ApprovedPlanSnapshot? lastApproved)
    {
        if (lastApproved is not null
            && FingerprintsMatch(currentProgress, currentProposedPlan, lastApproved))
        {
            return StalenessEvaluation.UpToDate;
        }

        if (lookbackBaseline is null)
        {
            return new StalenessEvaluation(true, ApprovalRequiredBecause.Both);
        }

        bool progressChangedSinceLookback =
            !string.Equals(
                currentProgress.Fingerprint,
                lookbackBaseline.ProgressRevision.Fingerprint,
                StringComparison.Ordinal);

        bool planChangedSinceLookback =
            !string.Equals(
                currentProposedPlan.Fingerprint,
                lookbackBaseline.PlanSnapshot.Fingerprint,
                StringComparison.Ordinal);

        if (!progressChangedSinceLookback && !planChangedSinceLookback)
        {
            return StalenessEvaluation.UpToDate;
        }

        ApprovalRequiredBecause because = (progressChangedSinceLookback, planChangedSinceLookback) switch
        {
            (true, true) => ApprovalRequiredBecause.Both,
            (true, false) => ApprovalRequiredBecause.ProgressChanged,
            (false, true) => ApprovalRequiredBecause.PlanRecalculated,
            _ => ApprovalRequiredBecause.None,
        };

        return new StalenessEvaluation(true, because);
    }

    private static bool FingerprintsMatch(
        ProgressRevisionRef currentProgress,
        PlanSnapshot currentProposedPlan,
        ApprovedPlanSnapshot lastApproved) =>
        string.Equals(currentProgress.Fingerprint, lastApproved.ProgressRevision.Fingerprint, StringComparison.Ordinal)
        && string.Equals(currentProposedPlan.Fingerprint, lastApproved.PlanSnapshot.Fingerprint, StringComparison.Ordinal);
}
