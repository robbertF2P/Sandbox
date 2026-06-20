namespace ImportPipeline.Domain;

/// <summary>
/// Immutable representation of a database ImportConfigRule row.
/// Acts as a specification: Apply() encodes the rule's decision logic
/// and returns a typed RuleResult — no ref parameters, no nulls-as-signals.
/// </summary>
public sealed record ImportConfigRule(
    int Id,
    string? From,
    string? To,
    string? DefaultValue,
    bool IsRequired,
    bool SkipIfEmpty,
    bool UseShortValue,
    ImportConfigRuleType RuleType)
{
    /// <summary>
    /// Applies this rule to a row. Pure function — no I/O, no mutation.
    /// Mirrors ExcelImportHelper.GetCellValue logic but with honest return types.
    /// </summary>
    public RuleResult Apply(ImportRow row)
    {
        // Degenerate rules (e.g. blank rows inserted in the database)
        if (string.IsNullOrEmpty(From) || string.IsNullOrEmpty(To))
            return new RuleResult.NoOp();

        var rawValue = row.GetCell(From);

        // Prefer raw cell value; fall back to DefaultValue when cell is absent or empty.
        var effective = Option.FromNullOrEmpty(rawValue)
                              .GetValueOrDefault(DefaultValue ?? string.Empty);

        if (string.IsNullOrEmpty(effective))
        {
            if (SkipIfEmpty)
                return new RuleResult.SkipRow($"Column '{From}' is empty — SkipIfEmpty triggered.");
            if (IsRequired)
                return new RuleResult.RequiredMissing(To);
            return new RuleResult.Absent(To);
        }

        var finalValue = UseShortValue ? TakeFirstWord(effective) : effective;
        return new RuleResult.Mapped(new FieldValue(To, finalValue, RuleType));
    }

    // IFS activity codes often look like "STARTED Some description" — UseShortValue takes only "STARTED".
    private static string TakeFirstWord(string value)
    {
        var space = value.IndexOf(' ');
        return space < 0 ? value : value[..space];
    }
}
