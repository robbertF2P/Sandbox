namespace Platform.Shared.Domain;

public readonly record struct UserName
{
    public string Value { get; }

    public UserName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("User name is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
