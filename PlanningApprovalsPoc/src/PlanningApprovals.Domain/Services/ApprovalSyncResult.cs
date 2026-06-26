namespace PlanningApprovals.Domain.Services;

public sealed record ApprovalSyncResult(IReadOnlyList<ApprovalSyncAction> Actions)
{
    public static ApprovalSyncResult NoAction { get; } = new([]);

    public bool RequiresPersistence => Actions.Count > 0;
}
