using Moq;
using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Npoi;
using PrimaveraExcelReader.Primavera.Profiles;

namespace PrimaveraExcelReader.Tests;

public sealed class ExcelReadIssueCollectionTests
{
    [Fact]
    public async Task ReadAsync_ContinuesAfterBadRowsAndCollectsAllIssues()
    {
        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet(
                "Activities",
                [
                    ["Activity ID", "Activity Name", "WBS Code", "Status", "Planned Start", "Planned Finish", "Original Duration (h)"],
                    ["A-100", "Valid Activity", "WBS-100", "Active", "2026-03-01", "2026-03-15", "120"],
                    ["A-200", "Bad Duration", "WBS-200", "Active", "2026-03-01", "2026-03-15", "not-a-number"],
                    ["", "Missing Id", "WBS-300", "Active", "2026-03-01", "2026-03-15", "80"]
                ])
            .BuildStream();

        var service = new ExcelReaderService(new NpoiExcelWorkbookAccessor());

        ExcelReadResult<PrimaveraExcelReader.Primavera.Models.PrimaveraActivityRow> result = await service.ReadAsync(
            stream,
            PrimaveraSheetProfiles.StandardActivityExport);

        Assert.Single(result.Rows);
        Assert.Equal("A-100", result.Rows[0].ActivityId);
        Assert.Equal(2, result.Issues.Count);
        Assert.Contains(result.Issues, issue => issue.Kind == ExcelReadIssueKind.ParseError && issue.RowNumber == 3);
        Assert.Contains(result.Issues, issue => issue.Kind == ExcelReadIssueKind.RequiredValueMissing && issue.RowNumber == 4);
    }

    [Fact]
    public async Task ReadAsync_CollectsMultipleColumnIssuesOnSameRow()
    {
        ExcelSheetProfile<MultiFieldRow> profile = ExcelSheetProfile<MultiFieldRow>.Configure()
            .Sheet("Rows")
            .Map(row => row.Code).From("Code", required: true)
            .Map(row => row.Hours).From("Hours")
            .Map(row => row.StartDate).From("Start")
            .Build();

        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet(
                "Rows",
                [
                    ["Code", "Hours", "Start"],
                    ["A-1", "bad-hours", "bad-date"]
                ])
            .BuildStream();

        var service = new ExcelReaderService(new NpoiExcelWorkbookAccessor());
        ExcelReadResult<MultiFieldRow> result = await service.ReadAsync(stream, profile);

        Assert.Empty(result.Rows);
        Assert.Equal(2, result.Issues.Count);
        Assert.All(result.Issues, issue => Assert.Equal(2, issue.RowNumber));
    }

    [Fact]
    public async Task ReadAsync_ReturnsIssueWhenSheetIsMissing()
    {
        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet("OtherSheet", [["Code"], ["A-1"]])
            .BuildStream();

        ExcelSheetProfile<MultiFieldRow> profile = ExcelSheetProfile<MultiFieldRow>.Configure()
            .Sheet("MissingSheet")
            .Map(row => row.Code).From("Code")
            .Build();

        var service = new ExcelReaderService(new NpoiExcelWorkbookAccessor());
        ExcelReadResult<MultiFieldRow> result = await service.ReadAsync(stream, profile);

        Assert.Empty(result.Rows);
        ExcelReadIssue issue = Assert.Single(result.Issues);
        Assert.Equal(ExcelReadIssueKind.SheetNotFound, issue.Kind);
    }

    [Fact]
    public async Task ReadAsync_FormatsIssuesForReporting()
    {
        Mock<IExcelWorkbookAccessor> accessorMock = ExcelReaderMoqExtensions.CreateWorkbookAccessorMock(
            "Activities",
            ExcelReaderMoqExtensions.CreateRow(
                1,
                ("Activity ID", "A-400"),
                ("Activity Name", "Tank Testing"),
                ("WBS Code", "WBS-400"),
                ("Status", null),
                ("Planned Start", null),
                ("Planned Finish", null),
                ("Original Duration (h)", "invalid")));

        var service = new ExcelReaderService(accessorMock.Object);

        ExcelReadResult<PrimaveraExcelReader.Primavera.Models.PrimaveraActivityRow> result = await service.ReadAsync(
            Stream.Null,
            PrimaveraSheetProfiles.StandardActivityExport);

        Assert.Empty(result.Rows);
        string report = ExcelReadReport.FormatIssues(result.Issues);
        Assert.Contains("Row 2", report, StringComparison.Ordinal);
        Assert.Contains("Original Duration (h)", report, StringComparison.Ordinal);
    }

    private sealed class MultiFieldRow
    {
        public string Code { get; set; } = string.Empty;

        public decimal? Hours { get; set; }

        public DateOnly? StartDate { get; set; }
    }
}
