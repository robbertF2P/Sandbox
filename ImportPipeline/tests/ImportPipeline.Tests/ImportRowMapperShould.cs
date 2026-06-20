using ImportPipeline.Domain;
using Xunit;

namespace ImportPipeline.Tests;

/// Tests for ImportRowMapper — the composition layer that folds N rule results into one row outcome.
public class ImportRowMapperShould
{
    [Fact]
    public void Return_mapped_with_all_values_when_all_rules_pass()
    {
        var mapper = new ImportRowMapper([
            Rule(1, from: "Project ID",   to: "Project.PlanningCode"),
            Rule(2, from: "Activity ID",  to: "Activity.PlanningCode"),
            Rule(3, from: "Status",       to: "Activity.Status"),
        ]);

        var row = ImportRow.From(
            ("Project ID",  "P001"),
            ("Activity ID", "A100"),
            ("Status",      "DONE"));

        var result = mapper.Map(row);

        var mapped = Assert.IsType<RowMappingResult.Mapped>(result);
        Assert.Equal(3, mapped.Values.Count);
        Assert.Contains(mapped.Values, v => v.FieldName == "Project.PlanningCode" && v.Value == "P001");
        Assert.Contains(mapped.Values, v => v.FieldName == "Activity.PlanningCode" && v.Value == "A100");
        Assert.Contains(mapped.Values, v => v.FieldName == "Activity.Status" && v.Value == "DONE");
    }

    [Fact]
    public void Return_skipped_when_any_skip_if_empty_rule_fires()
    {
        var mapper = new ImportRowMapper([
            Rule(1, from: "Task Code", to: "Activity.PlanningCode", skipIfEmpty: true),
            Rule(2, from: "Status",    to: "Activity.Status"),
        ]);

        var row = ImportRow.From(
            ("Task Code", ""),   // triggers SkipIfEmpty
            ("Status",    "DONE"));

        var result = mapper.Map(row);

        Assert.IsType<RowMappingResult.Skipped>(result);
    }

    [Fact]
    public void Return_invalid_listing_all_missing_required_fields()
    {
        var mapper = new ImportRowMapper([
            Rule(1, from: "Activity ID", to: "Activity.PlanningCode", isRequired: true),
            Rule(2, from: "Status",      to: "Activity.Status",        isRequired: true),
            Rule(3, from: "Project ID",  to: "Project.PlanningCode"),  // not required
        ]);

        var row = ImportRow.From(
            ("Project ID", "P001")); // Activity ID and Status are both missing

        var result = mapper.Map(row);

        var invalid = Assert.IsType<RowMappingResult.Invalid>(result);
        Assert.Contains("Activity.PlanningCode", invalid.MissingFields);
        Assert.Contains("Activity.Status",        invalid.MissingFields);
        Assert.Equal(2, invalid.MissingFields.Count);
    }

    [Fact]
    public void Skipped_takes_priority_over_invalid()
    {
        // A row that would be both skipped AND invalid: skip wins.
        var mapper = new ImportRowMapper([
            Rule(1, from: "Task Code", to: "Activity.PlanningCode", skipIfEmpty: true),
            Rule(2, from: "Status",    to: "Activity.Status",        isRequired: true),
        ]);

        var row = ImportRow.From(("Task Code", "")); // no Status either

        Assert.IsType<RowMappingResult.Skipped>(mapper.Map(row));
    }

    [Fact]
    public void Absent_non_required_fields_do_not_appear_in_mapped_values()
    {
        var mapper = new ImportRowMapper([
            Rule(1, from: "Status", to: "Activity.Status"),
            Rule(2, from: "Notes",  to: "Activity.Notes"),  // not in row
        ]);

        var row = ImportRow.From(("Status", "DONE"));

        var mapped = Assert.IsType<RowMappingResult.Mapped>(mapper.Map(row));
        Assert.Single(mapped.Values);
        Assert.Equal("Activity.Status", mapped.Values[0].FieldName);
    }

    [Fact]
    public void Noop_rules_are_silently_ignored()
    {
        var mapper = new ImportRowMapper([
            Rule(1, from: "Status",  to: "Activity.Status"),
            Rule(2, from: null,      to: null),   // blank / degenerate
        ]);

        var row = ImportRow.From(("Status", "DONE"));

        var mapped = Assert.IsType<RowMappingResult.Mapped>(mapper.Map(row));
        Assert.Single(mapped.Values);
    }

    [Fact]
    public void Multiple_rules_reading_the_same_source_column_all_produce_values()
    {
        // IFS config reads "Status" twice: once for Activity.Status (Attribute)
        // and once for Activity.Properties.IFS_Status (Property).
        var mapper = new ImportRowMapper([
            Rule(1, from: "Status", to: "Activity.Status",                 ruleType: ImportConfigRuleType.Attribute),
            Rule(2, from: "Status", to: "Activity.Properties.IFS_Status",  ruleType: ImportConfigRuleType.Property, isRequired: true),
        ]);

        var row = ImportRow.From(("Status", "STARTED"));

        var mapped = Assert.IsType<RowMappingResult.Mapped>(mapper.Map(row));
        Assert.Equal(2, mapped.Values.Count);
        Assert.Contains(mapped.Values, v => v.RuleType == ImportConfigRuleType.Attribute);
        Assert.Contains(mapped.Values, v => v.RuleType == ImportConfigRuleType.Property);
    }

    [Fact]
    public void MapAll_partitions_batch_results_by_outcome()
    {
        var mapper = new ImportRowMapper([
            Rule(1, from: "Activity ID", to: "Activity.PlanningCode", isRequired: true),
            Rule(2, from: "Status",      to: "Activity.Status",        skipIfEmpty: true),
        ]);

        var rows = new[]
        {
            ImportRow.From(("Activity ID", "A1"), ("Status", "DONE")),   // ok
            ImportRow.From(("Activity ID", "A2"), ("Status", "")),        // skipped
            ImportRow.From(("Status", "DONE")),                           // invalid (no Activity ID)
        };

        var (succeeded, skipped, invalid) = mapper.MapAll(rows);

        Assert.Single(succeeded);
        Assert.Single(skipped);
        Assert.Single(invalid);
    }

    [Fact]
    public void Field_values_can_be_partitioned_by_rule_type()
    {
        var mapper = new ImportRowMapper([
            Rule(1, from: "Project ID",   to: "Project.PlanningCode",              ruleType: ImportConfigRuleType.Attribute),
            Rule(2, from: "Activity ID",  to: "Activity.PlanningCode",             ruleType: ImportConfigRuleType.Attribute),
            Rule(3, from: "Status",       to: "Activity.Properties.IFS_Status",    ruleType: ImportConfigRuleType.Property),
        ]);

        var row = ImportRow.From(("Project ID", "P1"), ("Activity ID", "A1"), ("Status", "DONE"));
        var mapped = (RowMappingResult.Mapped)mapper.Map(row);

        Assert.Equal(2, mapped.Values.Attributes().Count);
        Assert.Single(mapped.Values.Properties());
    }

    // ── Builder helper ────────────────────────────────────────────────────────

    private static ImportConfigRule Rule(
        int id,
        string? from = null,
        string? to = null,
        string? defaultValue = null,
        bool isRequired = false,
        bool skipIfEmpty = false,
        bool useShortValue = false,
        ImportConfigRuleType ruleType = ImportConfigRuleType.Attribute) =>
        new(id, from, to, defaultValue, isRequired, skipIfEmpty, useShortValue, ruleType);
}
