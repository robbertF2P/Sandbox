using ImportPipeline.Domain;
using ImportPipeline.Domain.Specifications;
using Xunit;

namespace ImportPipeline.Tests;

/// Real-world scenario tests using the actual ImportConfig Id=12 ("IFS Activity Status import").
///
/// Rules reconstructed from the database (ImportConfigId=12):
///   97  Project ID         → Project.PlanningCode           Attribute
///   98  Activity ID        → Activity.PlanningCode          Attribute
///   99  Activity Sequence  → ifs_activity_sequence          Property
///   100 Activity Short Nam → ifs_activity_short_name        Property
///   101 Status             → Activity.Status                Attribute  (UseShortValue=true)
///   102 Sub Project ID     → Component.PlanningCode         Attribute
///   104 Status             → Activity.Properties.IFS_Status Property   IsRequired=true
///   111 (null)             → (null)                         Property   (blank/degenerate)
public class IfsActivityStatusImportShould
{
    // ── Config fixture ────────────────────────────────────────────────────────

    private static readonly IReadOnlyList<ImportConfigRule> IfsRules =
    [
        new(97,  "Project ID",          "Project.PlanningCode",               null,  false, false, false, ImportConfigRuleType.Attribute),
        new(98,  "Activity ID",         "Activity.PlanningCode",              null,  false, false, false, ImportConfigRuleType.Attribute),
        new(99,  "Activity Sequence",   "ifs_activity_sequence",              null,  false, false, false, ImportConfigRuleType.Property),
        new(100, "Activity Short Name", "ifs_activity_short_name",            null,  false, false, false, ImportConfigRuleType.Property),
        new(101, "Status",              "Activity.Status",                    null,  false, false, true,  ImportConfigRuleType.Attribute),
        new(102, "Sub Project ID",      "Component.PlanningCode",             null,  false, false, false, ImportConfigRuleType.Attribute),
        new(104, "Status",              "Activity.Properties.IFS_Status",     null,  true,  false, false, ImportConfigRuleType.Property),
        new(111, null,                  null,                                 null,  false, false, false, ImportConfigRuleType.Property),
    ];

    private static readonly ImportRowMapper Mapper = new(IfsRules);

    // ── Happy path ───────────────────────────────────────────────────────────

    [Fact]
    public void Map_complete_ifs_row_to_all_expected_fields()
    {
        var row = IfsRow(
            projectId: "PRJ-001",
            activityId: "ACT-001",
            sequence: "10",
            shortName: "Weld flange",
            status: "STARTED",
            subProjectId: "SP-01");

        var result = Mapper.Map(row);

        var mapped = Assert.IsType<RowMappingResult.Mapped>(result);

        AssertField(mapped, "Project.PlanningCode",             "PRJ-001");
        AssertField(mapped, "Activity.PlanningCode",            "ACT-001");
        AssertField(mapped, "ifs_activity_sequence",            "10");
        AssertField(mapped, "ifs_activity_short_name",          "Weld flange");
        AssertField(mapped, "Activity.Status",                  "STARTED");   // UseShortValue — no space to trim
        AssertField(mapped, "Component.PlanningCode",           "SP-01");
        AssertField(mapped, "Activity.Properties.IFS_Status",   "STARTED");
    }

    [Fact]
    public void Trim_status_to_first_word_for_attribute_mapping()
    {
        // IFS exports status as e.g. "STARTED 45% complete" — rule 101 uses UseShortValue.
        var row = IfsRow(status: "STARTED 45% complete");

        var mapped = (RowMappingResult.Mapped)Mapper.Map(row);

        // Rule 101 (Activity.Status): UseShortValue=true → first word only
        AssertField(mapped, "Activity.Status", "STARTED");
        // Rule 104 (IFS_Status): UseShortValue=false → full value preserved
        AssertField(mapped, "Activity.Properties.IFS_Status", "STARTED 45% complete");
    }

    [Fact]
    public void Ignore_blank_rule_111_silently()
    {
        var row = IfsRow();
        var mapped = (RowMappingResult.Mapped)Mapper.Map(row);

        // Rule 111 (null/null) should produce no field value
        Assert.DoesNotContain(mapped.Values, v => v.FieldName is null or "");
    }

    // ── Status is the only required field ────────────────────────────────────

    [Fact]
    public void Return_invalid_when_status_column_is_absent()
    {
        // This reproduces the real bug scenario: an IFS file where the Status
        // column has a different header or is missing entirely.
        var row = ImportRow.From(
            ("Project ID",  "PRJ-001"),
            ("Activity ID", "ACT-001"));   // no Status column at all

        var result = Mapper.Map(row);

        var invalid = Assert.IsType<RowMappingResult.Invalid>(result);
        Assert.Contains("Activity.Properties.IFS_Status", invalid.MissingFields);
    }

    [Fact]
    public void Return_invalid_when_status_column_is_empty()
    {
        var row = IfsRow(status: "");

        var result = Mapper.Map(row);

        Assert.IsType<RowMappingResult.Invalid>(result);
    }

    // ── Optional fields are gracefully absent ─────────────────────────────────

    [Fact]
    public void Succeed_when_only_required_and_identifier_columns_are_present()
    {
        // Minimal valid row: just enough for the import to work.
        var row = ImportRow.From(
            ("Status", "COMPLETED"));

        var result = Mapper.Map(row);

        // Should succeed — all other columns are non-required.
        var mapped = Assert.IsType<RowMappingResult.Mapped>(result);
        Assert.Contains(mapped.Values, v => v.FieldName == "Activity.Properties.IFS_Status");
    }

    // ── Specification layer ───────────────────────────────────────────────────

    [Fact]
    public void Specification_correctly_identifies_row_with_missing_required_field()
    {
        var spec = new AllRequiredFieldsPresentSpecification(IfsRules);

        var rowWithStatus    = IfsRow(status: "DONE");
        var rowWithoutStatus = ImportRow.From(("Project ID", "P1"));

        Assert.True(spec.IsSatisfiedBy(rowWithStatus));
        Assert.False(spec.IsSatisfiedBy(rowWithoutStatus));
    }

    [Fact]
    public void Specification_can_be_composed_with_And()
    {
        // A row is "processable" when: all required fields present AND not marked to skip.
        // (No SkipIfEmpty rules in the IFS config, so the skip spec never fires here —
        //  but the composition itself is verified.)
        IRowSpecification isComplete  = new AllRequiredFieldsPresentSpecification(IfsRules);
        IRowSpecification shouldSkip  = new RowShouldBeSkippedSpecification(IfsRules);
        IRowSpecification isProcessable = isComplete.And(shouldSkip.Not());

        Assert.True(isProcessable.IsSatisfiedBy(IfsRow(status: "DONE")));
        Assert.False(isProcessable.IsSatisfiedBy(ImportRow.From(("Project ID", "P1")))); // no status
    }

    // ── Batch processing ──────────────────────────────────────────────────────

    [Fact]
    public void MapAll_handles_a_mixed_batch_of_ifs_rows()
    {
        var rows = new[]
        {
            IfsRow(projectId: "P1", activityId: "A1", status: "STARTED"),
            IfsRow(projectId: "P2", activityId: "A2", status: "COMPLETED"),
            ImportRow.From(("Project ID", "P3")),   // missing Status → Invalid
        };

        var (succeeded, skipped, invalid) = Mapper.MapAll(rows);

        Assert.Equal(2, succeeded.Count);
        Assert.Empty(skipped);   // no SkipIfEmpty rules in IFS config
        Assert.Single(invalid);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ImportRow IfsRow(
        string projectId    = "PRJ-001",
        string activityId   = "ACT-001",
        string sequence     = "10",
        string shortName    = "Test activity",
        string status       = "STARTED",
        string subProjectId = "SP-01") =>
        ImportRow.From(
            ("Project ID",          projectId),
            ("Activity ID",         activityId),
            ("Activity Sequence",   sequence),
            ("Activity Short Name", shortName),
            ("Status",              status),
            ("Sub Project ID",      subProjectId));

    private static void AssertField(RowMappingResult.Mapped result, string fieldName, string expectedValue)
    {
        var field = result.Values.FirstOrDefault(v => v.FieldName == fieldName);
        Assert.True(field != default, $"Expected field '{fieldName}' in mapped values.");
        Assert.Equal(expectedValue, field.Value);
    }
}
