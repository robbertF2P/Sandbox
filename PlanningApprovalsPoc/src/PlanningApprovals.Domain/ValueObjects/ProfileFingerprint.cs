namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct ProfileFingerprint
{
    public string Value { get; }

    public ProfileFingerprint(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Profile fingerprint is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
