namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct ProjectId(long Value)
{
    public override string ToString() => Value.ToString();
}

public readonly record struct AssignmentId(long Value)
{
    public override string ToString() => Value.ToString();
}

public readonly record struct PersonId(long Value)
{
    public override string ToString() => Value.ToString();
}

public readonly record struct ProgressRevisionId(long Value)
{
    public override string ToString() => Value.ToString();
}

public readonly record struct ApprovalPublicId(Guid Value)
{
    public override string ToString() => Value.ToString();
}
