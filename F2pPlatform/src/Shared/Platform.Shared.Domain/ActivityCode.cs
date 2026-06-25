namespace Platform.Shared.Domain;

public readonly record struct ActivityCode
{
    public string Value { get; }

    public ActivityCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Activity code is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
