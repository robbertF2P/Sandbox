namespace ImportPipeline.Domain.Specifications;

/// <summary>
/// A row satisfies this spec (should be skipped) when any SkipIfEmpty rule
/// has an empty source column. Mirrors the early-exit logic that currently
/// lives inside the import job loop via a ref bool skip parameter.
/// </summary>
public sealed class RowShouldBeSkippedSpecification(IReadOnlyList<ImportConfigRule> rules)
    : IRowSpecification
{
    public bool IsSatisfiedBy(ImportRow row) =>
        rules
            .Where(r => r.SkipIfEmpty && !string.IsNullOrEmpty(r.From))
            .Any(r => string.IsNullOrEmpty(row.GetCell(r.From!)));
}
