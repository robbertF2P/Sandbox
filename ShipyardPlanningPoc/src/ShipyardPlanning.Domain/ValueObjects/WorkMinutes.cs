namespace ShipyardPlanning.Domain.ValueObjects;

public readonly record struct WorkMinutes(int Value)
{
    public TimeSpan ToTimeSpan() => TimeSpan.FromMinutes(Value);

    public static WorkMinutes operator +(WorkMinutes left, WorkMinutes right) =>
        new(left.Value + right.Value);
}
