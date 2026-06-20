namespace ImportPipeline.Domain;

/// <summary>
/// A successfully mapped field: which F2P field, what value, and how it should be applied.
/// Immutable value object — downstream code reads it, never mutates it.
/// </summary>
public readonly record struct FieldValue(
    string FieldName,
    string Value,
    ImportConfigRuleType RuleType);

public static class FieldValueExtensions
{
    public static IReadOnlyList<FieldValue> Attributes(this IEnumerable<FieldValue> values) =>
        values.Where(v => v.RuleType == ImportConfigRuleType.Attribute).ToList();

    public static IReadOnlyList<FieldValue> Properties(this IEnumerable<FieldValue> values) =>
        values.Where(v => v.RuleType == ImportConfigRuleType.Property).ToList();

    public static IReadOnlyList<FieldValue> Structures(this IEnumerable<FieldValue> values) =>
        values.Where(v => v.RuleType == ImportConfigRuleType.Structure).ToList();
}
