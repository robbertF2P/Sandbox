namespace ShipyardPlanning.Domain.ValueObjects;

public readonly record struct BlockCode(string Value)
{
    public override string ToString() => Value;
}
