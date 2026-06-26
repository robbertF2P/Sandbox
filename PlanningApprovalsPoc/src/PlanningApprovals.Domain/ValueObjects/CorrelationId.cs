namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct CorrelationId
{
    public string Value { get; }

    public CorrelationId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Correlation id is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
