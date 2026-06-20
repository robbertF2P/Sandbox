namespace ImportPipeline.Domain;

/// <summary>
/// Discriminated union for the outcome of applying one ImportConfigRule to one row.
///
/// Mapped          — value found (or defaulted), ready for downstream use.
/// Absent          — column missing and not required; row processing continues.
/// RequiredMissing — column missing and IsRequired=true; whole row becomes Invalid.
/// SkipRow         — SkipIfEmpty fired; whole row should be ignored.
/// NoOp            — rule is blank/degenerate (null From/To); silently ignored.
/// </summary>
public abstract record RuleResult
{
    public sealed record Mapped(FieldValue Value) : RuleResult;
    public sealed record Absent(string FieldName) : RuleResult;
    public sealed record RequiredMissing(string FieldName) : RuleResult;
    public sealed record SkipRow(string Reason) : RuleResult;
    public sealed record NoOp : RuleResult;
}
