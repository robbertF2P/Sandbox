using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Primavera.Models;
using PrimaveraExcelReader.Primavera.Profiles;

namespace PrimaveraExcelReader.Tests;

public sealed class ExcelReadIssueCollectionTests
{
    [Fact]
    public async Task ReadAsync_ContinuesAfterBadRowsAndCollectsAllIssues()
    {
        ExcelReadResult<PrimaveraActivityRow> result = await ExcelReaderTestRunner.ReadSheetAsync(
            PrimaveraSheetScenarios.MixedQualityActivitiesSheet,
            PrimaveraSheetProfiles.StandardActivityExport);

        ExcelReadResultAssert.HasRows(result, 1);
        Assert.Equal("A-100", result.Rows[0].ActivityId);
        ExcelReadResultAssert.HasIssue(result.Issues, ExcelReadIssueKind.ParseError, 3);
        ExcelReadResultAssert.HasIssue(result.Issues, ExcelReadIssueKind.RequiredValueMissing, 4);
    }

    [Fact]
    public async Task ReadAsync_CollectsMultipleColumnIssuesOnSameRow()
    {
        ExcelSheetProfile<TypedSampleRow> profile = ExcelSheetProfile<TypedSampleRow>.Configure()
            .Sheet("Rows")
            .Map(row => row.Code).From("Code", required: true)
            .Map(row => row.Hours).From("Hours")
            .Map(row => row.StartDate).From("Start")
            .Build();

        ExcelReadResult<TypedSampleRow> result = await ExcelReaderTestRunner.ReadSheetAsync(
            new TestSheetDefinition(
                "Rows",
                [
                    ["Code", "Hours", "Start"],
                    ["A-1", "bad-hours", "bad-date"]
                ]),
            profile);

        Assert.Empty(result.Rows);
        Assert.Equal(2, result.Issues.Count);
        Assert.All(result.Issues, issue => Assert.Equal(2, issue.RowNumber));
    }

    [Fact]
    public async Task ReadAsync_ReturnsIssueWhenSheetIsMissing()
    {
        ExcelSheetProfile<TypedSampleRow> profile = ExcelSheetProfile<TypedSampleRow>.Configure()
            .Sheet("MissingSheet")
            .Map(row => row.Code).From("Code")
            .Build();

        ExcelReadResult<TypedSampleRow> result = await ExcelReaderTestRunner.ReadSheetAsync(
            new TestSheetDefinition("OtherSheet", [["Code"], ["A-1"]]),
            profile);

        Assert.Empty(result.Rows);
        ExcelReadIssue issue = Assert.Single(result.Issues);
        Assert.Equal(ExcelReadIssueKind.SheetNotFound, issue.Kind);
    }

    [Fact]
    public async Task ReadAsync_FormatsIssuesForReporting()
    {
        ExcelReadResult<PrimaveraActivityRow> result = await ExcelReaderTestRunner.ReadRowsAsync(
            PrimaveraSheetProfiles.StandardActivityExport,
            ExcelRowFactory.FromCells(
                1,
                ("Activity ID", "A-400"),
                ("Activity Name", "Tank Testing"),
                ("WBS Code", "WBS-400"),
                ("Status", null),
                ("Planned Start", null),
                ("Planned Finish", null),
                ("Original Duration (h)", "invalid")));

        Assert.Empty(result.Rows);
        string report = ExcelReadReport.FormatIssues(result.Issues);
        Assert.Contains("Row 2", report, StringComparison.Ordinal);
        Assert.Contains("Original Duration (h)", report, StringComparison.Ordinal);
    }

    private sealed class TypedSampleRow
    {
        public string Code { get; set; } = string.Empty;

        public decimal? Hours { get; set; }

        public DateOnly? StartDate { get; set; }
    }
}
