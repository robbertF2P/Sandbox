using PlanningApprovals.Domain.Enums;

namespace PlanningApprovals.Domain.Services;

public sealed record StalenessEvaluation(
    bool RequiresApproval,
    ApprovalRequiredBecause RequiredBecause)
{
    public static StalenessEvaluation UpToDate { get; } = new(false, ApprovalRequiredBecause.None);
}
