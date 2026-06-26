namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct ProjectId(long Value)
{
    public override string ToString() => Value.ToString();
}
