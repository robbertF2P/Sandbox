namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct ApprovalPublicId(Guid Value)
{
    public override string ToString() => Value.ToString();
}
