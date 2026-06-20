namespace ImportPipeline.Domain.Specifications;

/// <summary>
/// A row satisfies this spec (is complete) when every required rule
/// resolves to a non-empty value (raw cell or DefaultValue).
/// Can be used as a fast pre-filter before running the full mapper.
/// </summary>
public sealed class AllRequiredFieldsPresentSpecification(IReadOnlyList<ImportConfigRule> rules)
    : IRowSpecification
{
    public bool IsSatisfiedBy(ImportRow row) =>
        rules
            .Where(r => r.IsRequired && !string.IsNullOrEmpty(r.From))
            .All(r => !string.IsNullOrEmpty(
                Option.FromNullOrEmpty(row.GetCell(r.From!))
                      .GetValueOrDefault(r.DefaultValue ?? string.Empty)));
}
