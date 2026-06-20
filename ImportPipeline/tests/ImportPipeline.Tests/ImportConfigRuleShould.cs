using ImportPipeline.Domain;
using Xunit;

namespace ImportPipeline.Tests;

/// Unit tests for ImportConfigRule.Apply — one rule against one row.
/// Each test expresses a domain scenario, not an implementation detail.
public class ImportConfigRuleShould
{
    // ── Happy path ───────────────────────────────────────────────────────────

    [Fact]
    public void Return_mapped_value_when_column_is_present()
    {
        var rule = Rule(from: "Status", to: "Activity.Status");
        var row  = ImportRow.From(("Status", "COMPLETED"));

        var result = rule.Apply(row);

        var mapped = Assert.IsType<RuleResult.Mapped>(result);
        Assert.Equal("Activity.Status", mapped.Value.FieldName);
        Assert.Equal("COMPLETED", mapped.Value.Value);
    }

    [Fact]
    public void Preserve_rule_type_in_field_value()
    {
        var rule = Rule(from: "Status", to: "Activity.Properties.IFS_Status",
                        ruleType: ImportConfigRuleType.Property);
        var row = ImportRow.From(("Status", "STARTED"));

        var result = (RuleResult.Mapped)rule.Apply(row);

        Assert.Equal(ImportConfigRuleType.Property, result.Value.RuleType);
    }

    // ── DefaultValue ─────────────────────────────────────────────────────────

    [Fact]
    public void Fall_back_to_default_value_when_column_is_absent()
    {
        var rule = Rule(from: "Status", to: "Activity.Status", defaultValue: "PENDING");
        var row  = ImportRow.From(("Project ID", "P001")); // no Status column

        var result = rule.Apply(row);

        var mapped = Assert.IsType<RuleResult.Mapped>(result);
        Assert.Equal("PENDING", mapped.Value.Value);
    }

    [Fact]
    public void Fall_back_to_default_value_when_column_is_empty_string()
    {
        var rule = Rule(from: "Status", to: "Activity.Status", defaultValue: "PENDING");
        var row  = ImportRow.From(("Status", ""));

        var result = rule.Apply(row);

        var mapped = Assert.IsType<RuleResult.Mapped>(result);
        Assert.Equal("PENDING", mapped.Value.Value);
    }

    // ── IsRequired ───────────────────────────────────────────────────────────

    [Fact]
    public void Return_required_missing_when_required_column_is_absent()
    {
        var rule = Rule(from: "Status", to: "Activity.Properties.IFS_Status", isRequired: true);
        var row  = ImportRow.From(("Project ID", "P001")); // no Status column

        var result = rule.Apply(row);

        var missing = Assert.IsType<RuleResult.RequiredMissing>(result);
        Assert.Equal("Activity.Properties.IFS_Status", missing.FieldName);
    }

    [Fact]
    public void Return_required_missing_even_when_column_exists_but_is_empty()
    {
        var rule = Rule(from: "Status", to: "Activity.Properties.IFS_Status", isRequired: true);
        var row  = ImportRow.From(("Status", ""));

        var result = rule.Apply(row);

        Assert.IsType<RuleResult.RequiredMissing>(result);
    }

    // ── SkipIfEmpty ───────────────────────────────────────────────────────────

    [Fact]
    public void Return_skip_row_when_skip_if_empty_fires_on_empty_column()
    {
        var rule = Rule(from: "Task Code", to: "Activity.PlanningCode", skipIfEmpty: true);
        var row  = ImportRow.From(("Task Code", ""));

        var result = rule.Apply(row);

        Assert.IsType<RuleResult.SkipRow>(result);
    }

    [Fact]
    public void Return_skip_row_when_skip_if_empty_fires_on_absent_column()
    {
        var rule = Rule(from: "Task Code", to: "Activity.PlanningCode", skipIfEmpty: true);
        var row  = ImportRow.From(("Status", "DONE")); // no Task Code column

        var result = rule.Apply(row);

        Assert.IsType<RuleResult.SkipRow>(result);
    }

    // ── Absent (non-required, no default) ────────────────────────────────────

    [Fact]
    public void Return_absent_when_non_required_column_is_missing_and_no_default()
    {
        var rule = Rule(from: "Optional Notes", to: "Activity.Notes");
        var row  = ImportRow.From(("Status", "DONE")); // no Optional Notes column

        var result = rule.Apply(row);

        var absent = Assert.IsType<RuleResult.Absent>(result);
        Assert.Equal("Activity.Notes", absent.FieldName);
    }

    // ── UseShortValue ────────────────────────────────────────────────────────

    [Fact]
    public void Take_first_word_only_when_use_short_value_is_true()
    {
        var rule = Rule(from: "Status", to: "Activity.Status", useShortValue: true);
        var row  = ImportRow.From(("Status", "STARTED Some extra description"));

        var result = rule.Apply(row);

        var mapped = Assert.IsType<RuleResult.Mapped>(result);
        Assert.Equal("STARTED", mapped.Value.Value);
    }

    [Fact]
    public void Leave_value_unchanged_when_use_short_value_is_true_but_no_space_in_value()
    {
        var rule = Rule(from: "Status", to: "Activity.Status", useShortValue: true);
        var row  = ImportRow.From(("Status", "COMPLETED"));

        var result = rule.Apply(row);

        var mapped = Assert.IsType<RuleResult.Mapped>(result);
        Assert.Equal("COMPLETED", mapped.Value.Value);
    }

    // ── Degenerate / blank rules ──────────────────────────────────────────────

    [Fact]
    public void Return_noop_when_From_is_null()
    {
        var rule = Rule(from: null, to: "Activity.Status");
        var row  = ImportRow.From(("Status", "DONE"));

        Assert.IsType<RuleResult.NoOp>(rule.Apply(row));
    }

    [Fact]
    public void Return_noop_when_To_is_null()
    {
        var rule = Rule(from: "Status", to: null);
        var row  = ImportRow.From(("Status", "DONE"));

        Assert.IsType<RuleResult.NoOp>(rule.Apply(row));
    }

    [Fact]
    public void Return_noop_for_completely_blank_rule()
    {
        var rule = Rule(from: null, to: null); // mirrors rule 111 in IFS config Id=12
        var row  = ImportRow.From(("Status", "DONE"));

        Assert.IsType<RuleResult.NoOp>(rule.Apply(row));
    }

    // ── Column name matching is case-insensitive ──────────────────────────────

    [Fact]
    public void Match_column_names_case_insensitively()
    {
        var rule = Rule(from: "status", to: "Activity.Status");
        var row  = ImportRow.From(("STATUS", "DONE"));

        Assert.IsType<RuleResult.Mapped>(rule.Apply(row));
    }

    // ── Builder helper ────────────────────────────────────────────────────────

    private static ImportConfigRule Rule(
        string? from = null,
        string? to = null,
        string? defaultValue = null,
        bool isRequired = false,
        bool skipIfEmpty = false,
        bool useShortValue = false,
        ImportConfigRuleType ruleType = ImportConfigRuleType.Attribute) =>
        new(Id: 1, From: from, To: to, DefaultValue: defaultValue,
            IsRequired: isRequired, SkipIfEmpty: skipIfEmpty,
            UseShortValue: useShortValue, RuleType: ruleType);
}
