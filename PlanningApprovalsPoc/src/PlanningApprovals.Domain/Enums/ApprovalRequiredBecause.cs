namespace PlanningApprovals.Domain.Enums;

[Flags]
public enum ApprovalRequiredBecause
{
    None = 0,
    ProgressChanged = 1,
    PlanRecalculated = 2,
    Both = ProgressChanged | PlanRecalculated,
}
