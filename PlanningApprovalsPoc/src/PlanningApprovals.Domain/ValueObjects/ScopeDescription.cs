namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct ScopeDescription
{
    public string Value { get; }

    public ScopeDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Scope description is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
