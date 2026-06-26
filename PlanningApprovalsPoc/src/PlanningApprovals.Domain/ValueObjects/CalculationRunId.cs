namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct CalculationRunId
{
    public string Value { get; }

    public CalculationRunId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Calculation run id is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
