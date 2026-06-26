namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct ProgressSource
{
    public string Value { get; }

    public ProgressSource(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Progress source is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public static ProgressSource Timesheet { get; } = new("Timesheet");

    public override string ToString() => Value;
}
