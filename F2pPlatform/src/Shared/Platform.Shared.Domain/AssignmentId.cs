namespace Platform.Shared.Domain;

public readonly record struct AssignmentId(Guid Value)
{
    public override string ToString() => Value.ToString();
}
