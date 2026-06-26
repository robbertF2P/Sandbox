namespace Platform.Shared.Domain;

public readonly record struct ProjectLabel
{
    public string Value { get; }

    public ProjectLabel(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Project label is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
