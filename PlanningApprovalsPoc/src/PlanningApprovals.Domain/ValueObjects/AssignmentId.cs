namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct AssignmentId(long Value)
{
    public override string ToString() => Value.ToString();
}
