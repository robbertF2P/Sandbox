using Moq;
using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Primavera.Models;
using PrimaveraExcelReader.Primavera.Profiles;

namespace PrimaveraExcelReader.Tests;

public sealed class ExcelReaderServiceTests
{
    [Fact]
    public async Task ReadAsync_MapsStandardPrimaveraActivitySheet()
    {
        ExcelReadResult<PrimaveraActivityRow> result = await ExcelReaderTestRunner.ReadSheetAsync(
            PrimaveraSheetScenarios.StandardActivitiesSheet,
            PrimaveraSheetProfiles.StandardActivityExport);

        ExcelReadResultAssert.HasRows(result, 2);

        PrimaveraActivityRow first = result.Rows[0];
        Assert.Equal("A-100", first.ActivityId);
        Assert.Equal("Hull Block Erection", first.ActivityName);
        Assert.Equal("WBS-204", first.WbsCode);
        Assert.Equal(120m, first.DurationHours);
        Assert.Equal(new DateOnly(2026, 3, 1), first.PlannedStart);
    }

    [Fact]
    public async Task ReadAsync_UsesMockedWorkbookAccessorForIsolatedMappingTests()
    {
        ExcelRowData row = ExcelRowFactory.FromCells(
            1,
            ("Activity ID", "A-300"),
            ("Activity Name", "Propulsion Alignment"),
            ("WBS Code", "WBS-301"),
            ("Status", "Complete"),
            ("Planned Start", "2026-02-01"),
            ("Planned Finish", "2026-02-10"),
            ("Original Duration (h)", "40"));

        ExcelReadResult<PrimaveraActivityRow> result = await ExcelReaderTestRunner.ReadRowsAsync(
            PrimaveraSheetProfiles.StandardActivityExport,
            row);

        PrimaveraActivityRow activity = Assert.Single(result.Rows);
        Assert.Equal("A-300", activity.ActivityId);
        Assert.Equal("Propulsion Alignment", activity.ActivityName);
    }

    [Fact]
    public async Task ReadAsync_SkipsEmptyRowsAndRecordsReason()
    {
        ExcelRowData validRow = ExcelRowFactory.FromCells(
            1,
            ("Activity ID", "A-400"),
            ("Activity Name", "Tank Testing"),
            ("WBS Code", "WBS-400"),
            ("Status", null),
            ("Planned Start", null),
            ("Planned Finish", null),
            ("Original Duration (h)", null));

        ExcelRowData emptyRow = ExcelRowFactory.FromCells(
            2,
            ("Activity ID", null),
            ("Activity Name", null),
            ("WBS Code", null),
            ("Status", null),
            ("Planned Start", null),
            ("Planned Finish", null),
            ("Original Duration (h)", null));

        ExcelReadResult<PrimaveraActivityRow> result = await ExcelReaderTestRunner.ReadRowsAsync(
            PrimaveraSheetProfiles.StandardActivityExport,
            validRow,
            emptyRow);

        Assert.Single(result.Rows);
        ExcelReadResultAssert.HasIssue(result.Issues, ExcelReadIssueKind.EmptyRow, 3);
    }

    [Fact]
    public async Task ReadAsync_MapsLegacyPrimaveraTaskSheetLayout()
    {
        ExcelReadResult<PrimaveraActivityRow> result = await ExcelReaderTestRunner.ReadSheetAsync(
            PrimaveraSheetScenarios.LegacyTaskSheet,
            PrimaveraSheetProfiles.LegacyActivityExport);

        PrimaveraActivityRow activity = Assert.Single(result.Rows);
        Assert.Equal("A-500", activity.ActivityId);
        Assert.Equal("Steel Cutting", activity.ActivityName);
        Assert.DoesNotContain(result.Rows, row => row.ActivityId == "M-001");
    }

    [Fact]
    public async Task ReadAsync_MapsResourceAssignmentSheetByColumnIndex()
    {
        ExcelReadResult<PrimaveraTaskRow> result = await ExcelReaderTestRunner.ReadSheetAsync(
            PrimaveraSheetScenarios.ResourceAssignmentSheet,
            PrimaveraSheetProfiles.ResourceAssignmentExport);

        PrimaveraTaskRow task = Assert.Single(result.Rows);
        Assert.Equal("T-100", task.TaskId);
        Assert.Equal("A-100", task.ActivityId);
        Assert.Equal("Welding crew lead", task.TaskName);
        Assert.Equal("RES-WELD-01", task.ResourceName);
        Assert.Equal(160m, task.BudgetedUnits);
        Assert.Equal(120m, task.RemainingUnits);
        Assert.Equal("WELD", task.TradeCode);
    }
}
