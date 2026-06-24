using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Services;

public sealed record StalenessEvaluation(
    bool RequiresApproval,
    ApprovalRequiredBecause RequiredBecause)
{
    public static StalenessEvaluation UpToDate { get; } = new(false, ApprovalRequiredBecause.None);
}

public static class ApprovalStalenessEvaluator
{
    public static StalenessEvaluation Evaluate(
        ProgressRevisionRef currentProgress,
        PlanSnapshot currentProposedPlan,
        ApprovedPlanSnapshot? lastApproved)
    {
        if (lastApproved is null)
        {
            return new StalenessEvaluation(true, ApprovalRequiredBecause.Both);
        }

        bool progressChanged =
            !string.Equals(
                currentProgress.Fingerprint,
                lastApproved.ProgressRevision.Fingerprint,
                StringComparison.Ordinal);

        bool planChanged =
            !string.Equals(
                currentProposedPlan.Fingerprint,
                lastApproved.PlanSnapshot.Fingerprint,
                StringComparison.Ordinal);

        if (!progressChanged && !planChanged)
        {
            return StalenessEvaluation.UpToDate;
        }

        ApprovalRequiredBecause because = (progressChanged, planChanged) switch
        {
            (true, true) => ApprovalRequiredBecause.Both,
            (true, false) => ApprovalRequiredBecause.ProgressChanged,
            (false, true) => ApprovalRequiredBecause.PlanRecalculated,
            _ => ApprovalRequiredBecause.None,
        };

        return new StalenessEvaluation(true, because);
    }
}
