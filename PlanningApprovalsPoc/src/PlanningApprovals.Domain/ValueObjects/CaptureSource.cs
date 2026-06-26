namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct CaptureSource
{
    public string Value { get; }

    public CaptureSource(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Capture source is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
