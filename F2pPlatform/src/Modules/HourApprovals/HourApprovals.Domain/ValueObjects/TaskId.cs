namespace HourApprovals.Domain.ValueObjects;

public readonly record struct TaskId(Guid Value)
{
    public override string ToString() => Value.ToString();
}
