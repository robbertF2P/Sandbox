namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct ProgressRevisionId(long Value)
{
    public override string ToString() => Value.ToString();
}
