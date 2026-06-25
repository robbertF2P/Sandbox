namespace Platform.Shared.Domain;

public readonly record struct TaskId(Guid Value)
{
    public override string ToString() => Value.ToString();
}

public readonly record struct AssignmentId(Guid Value)
{
    public override string ToString() => Value.ToString();
}

public readonly record struct OrganisationId(int Value)
{
    public override string ToString() => Value.ToString();
}
