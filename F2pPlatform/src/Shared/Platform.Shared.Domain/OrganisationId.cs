namespace Platform.Shared.Domain;

public readonly record struct OrganisationId(int Value)
{
    public override string ToString() => Value.ToString();
}
