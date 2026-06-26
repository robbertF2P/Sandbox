namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct ProcessName
{
    public string Value { get; }

    public ProcessName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Process name is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
