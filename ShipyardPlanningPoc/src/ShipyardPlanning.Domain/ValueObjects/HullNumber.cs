namespace ShipyardPlanning.Domain.ValueObjects;

public readonly record struct HullNumber(string Value)
{
    public override string ToString() => Value;
}
