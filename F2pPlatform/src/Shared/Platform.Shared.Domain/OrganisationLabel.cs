namespace Platform.Shared.Domain;

public readonly record struct OrganisationLabel
{
    public string Value { get; }

    public OrganisationLabel(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Organisation label is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
