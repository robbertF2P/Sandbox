using ImportPipeline.Domain;
using ImportPipeline.Domain.Specifications;
using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.ImportPipeline;
using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Primavera.Models;
using PrimaveraExcelReader.Primavera.Profiles;

namespace PrimaveraExcelReader.Tests;

public sealed class ImportPipelineIntegrationTests
{
    [Fact]
    public void ProfileRules_MatchStandardActivityExportBindings()
    {
        IReadOnlyList<ImportConfigRule> rules =
            ExcelSheetProfileImportRules.FromProfile(PrimaveraSheetProfiles.StandardActivityExport);

        Assert.Equal(7, rules.Count);
        Assert.Contains(rules, rule =>
            rule.From == "Activity ID"
            && rule.To == "ActivityId"
            && rule.IsRequired);
        Assert.Contains(rules, rule =>
            rule.From == "Original Duration (h)"
            && rule.To == "DurationHours"
            && !rule.IsRequired);
    }

    [Fact]
    public void TryMapRow_UsesImportPipelineMapper_ForPrimaveraActivity()
    {
        ExcelRowData row = ExcelRowFactory.FromHeaderRowAndValues(
            1,
            PrimaveraSheetScenarios.StandardActivityHeaders,
            ["A-100", "Hull Block Erection", "WBS-204", "In Progress", "2026-03-01", "2026-03-15", "120"]);

        ExcelRowMapResult<PrimaveraActivityRow> result =
            PrimaveraSheetProfiles.StandardActivityExport.TryMapRow(row);

        PrimaveraActivityRow activity = ExcelReadResultAssert.Success(result);
        Assert.Equal("A-100", activity.ActivityId);
        Assert.Equal(120m, activity.DurationHours);
        Assert.Equal(new DateOnly(2026, 3, 1), activity.PlannedStart);
    }

    [Fact]
    public void TryMapRow_ReportsAllMissingRequiredFieldsViaImportPipeline()
    {
        ExcelSheetProfile<TypedSampleRow> profile = ExcelSheetProfile<TypedSampleRow>.Configure()
            .Sheet("Rows")
            .Map(row => row.Code).From("Code", required: true)
            .Map(row => row.Name).From("Name", required: true)
            .Build();

        ExcelRowData row = ExcelRowFactory.FromCells(1, ("Status", "DONE"));

        ExcelRowMapResult<TypedSampleRow> result = profile.TryMapRow(row);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Issues.Count);
        Assert.All(result.Issues, issue => Assert.Equal(ExcelReadIssueKind.RequiredValueMissing, issue.Kind));
    }

    [Fact]
    public void RequiredFieldsSpecification_AgreesWithImportPipelineMapper()
    {
        IReadOnlyList<ImportConfigRule> rules =
            ExcelSheetProfileImportRules.FromProfile(PrimaveraSheetProfiles.StandardActivityExport);

        var spec = new AllRequiredFieldsPresentSpecification(rules);
        var mapper = new ImportRowMapper(rules);

        ExcelRowData completeRow = ExcelRowFactory.FromHeaderRowAndValues(
            1,
            PrimaveraSheetScenarios.StandardActivityHeaders,
            ["A-100", "Hull Block Erection", "WBS-204", "In Progress", "2026-03-01", "2026-03-15", "120"]);

        ExcelRowData incompleteRow = ExcelRowFactory.FromCells(
            2,
            ("Activity Name", "Missing IDs"));

        ImportRow completeImportRow = ImportRowAdapter.FromExcelRow(completeRow);
        ImportRow incompleteImportRow = ImportRowAdapter.FromExcelRow(incompleteRow);

        Assert.True(spec.IsSatisfiedBy(completeImportRow));
        Assert.False(spec.IsSatisfiedBy(incompleteImportRow));
        Assert.IsType<RowMappingResult.Mapped>(mapper.Map(completeImportRow));
        Assert.IsType<RowMappingResult.Invalid>(mapper.Map(incompleteImportRow));
    }

    private sealed class TypedSampleRow
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
