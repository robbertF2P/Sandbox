namespace ImportPipeline.Domain;

/// <summary>
/// Composes a list of ImportConfigRules into a single row-level mapping operation.
///
/// Each rule is applied independently (no short-circuit on first failure) so that
/// Invalid results can report ALL missing required fields at once — not just the first one.
///
/// Priority of outcomes: Skipped > Invalid > Mapped.
/// </summary>
public sealed class ImportRowMapper(IReadOnlyList<ImportConfigRule> rules)
{
    public RowMappingResult Map(ImportRow row)
    {
        var results = rules.Select(r => r.Apply(row)).ToList();

        var skipSignal = results.OfType<RuleResult.SkipRow>().FirstOrDefault();
        if (skipSignal is not null)
            return new RowMappingResult.Skipped(skipSignal.Reason);

        var missingRequired = results
            .OfType<RuleResult.RequiredMissing>()
            .Select(r => r.FieldName)
            .ToList();

        if (missingRequired.Count > 0)
            return new RowMappingResult.Invalid(missingRequired);

        var values = results
            .OfType<RuleResult.Mapped>()
            .Select(r => r.Value)
            .ToList();

        return new RowMappingResult.Mapped(values);
    }

    /// Convenience: map a batch of rows and partition into outcomes.
    public (
        IReadOnlyList<RowMappingResult.Mapped> Succeeded,
        IReadOnlyList<RowMappingResult.Skipped> Skipped,
        IReadOnlyList<RowMappingResult.Invalid> Invalid
    ) MapAll(IEnumerable<ImportRow> rows)
    {
        var results = rows.Select(Map).ToList();
        return (
            results.OfType<RowMappingResult.Mapped>().ToList(),
            results.OfType<RowMappingResult.Skipped>().ToList(),
            results.OfType<RowMappingResult.Invalid>().ToList()
        );
    }
}
