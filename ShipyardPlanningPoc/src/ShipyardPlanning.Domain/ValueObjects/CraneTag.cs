namespace ShipyardPlanning.Domain.ValueObjects;

public readonly record struct CraneTag(string Value)
{
    public override string ToString() => Value;
}
