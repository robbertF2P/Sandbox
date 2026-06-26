namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct DecisionComment
{
    public string Value { get; }

    public DecisionComment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Decision comment cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
