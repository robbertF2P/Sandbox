namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct PersonId(long Value)
{
    public override string ToString() => Value.ToString();
}
