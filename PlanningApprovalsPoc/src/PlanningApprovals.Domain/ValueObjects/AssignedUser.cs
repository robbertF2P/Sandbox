namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct AssignedUser(string Value)
{
    public override string ToString() => Value;

    public static AssignedUser From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Assigned user is required.", nameof(value));
        }

        return new AssignedUser(value.Trim());
    }
}
