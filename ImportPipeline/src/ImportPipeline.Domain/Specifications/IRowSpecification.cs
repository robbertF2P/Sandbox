namespace ImportPipeline.Domain.Specifications;

/// <summary>
/// Evans DDD: a predicate object that answers a named domain question about a row.
/// Implementations are composable with And / Or / Not without touching the original class.
/// </summary>
public interface IRowSpecification
{
    bool IsSatisfiedBy(ImportRow row);

    IRowSpecification And(IRowSpecification other) => new AndSpecification(this, other);
    IRowSpecification Or(IRowSpecification other)  => new OrSpecification(this, other);
    IRowSpecification Not()                        => new NotSpecification(this);
}

internal sealed class AndSpecification(IRowSpecification left, IRowSpecification right) : IRowSpecification
{
    public bool IsSatisfiedBy(ImportRow row) =>
        left.IsSatisfiedBy(row) && right.IsSatisfiedBy(row);
}

internal sealed class OrSpecification(IRowSpecification left, IRowSpecification right) : IRowSpecification
{
    public bool IsSatisfiedBy(ImportRow row) =>
        left.IsSatisfiedBy(row) || right.IsSatisfiedBy(row);
}

internal sealed class NotSpecification(IRowSpecification inner) : IRowSpecification
{
    public bool IsSatisfiedBy(ImportRow row) => !inner.IsSatisfiedBy(row);
}
