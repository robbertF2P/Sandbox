using Moq;
using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Npoi;
using PrimaveraExcelReader.Primavera.Models;
using PrimaveraExcelReader.Primavera.Profiles;

namespace PrimaveraExcelReader.Tests;

public sealed class ExcelReaderServiceTests
{
    [Fact]
    public async Task ReadAsync_MapsStandardPrimaveraActivitySheet()
    {
        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet(
                "Activities",
                [
                    ["Activity ID", "Activity Name", "WBS Code", "Status", "Planned Start", "Planned Finish", "Original Duration (h)"],
                    ["A-100", "Hull Block Erection", "WBS-204", "In Progress", "2026-03-01", "2026-03-15", "120"],
                    ["A-200", "Engine Room Outfitting", "WBS-205", "Not Started", "2026-03-16", "2026-04-01", "80"]
                ])
            .BuildStream();

        var service = new ExcelReaderService(new NpoiExcelWorkbookAccessor());

        ExcelReadResult<PrimaveraActivityRow> result = await service.ReadAsync(
            stream,
            PrimaveraSheetProfiles.StandardActivityExport);

        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("A-100", result.Rows[0].ActivityId);
        Assert.Equal("Hull Block Erection", result.Rows[0].ActivityName);
        Assert.Equal("WBS-204", result.Rows[0].WbsCode);
        Assert.Equal("120", result.Rows[0].DurationHours);
    }

    [Fact]
    public async Task ReadAsync_UsesMockedWorkbookAccessorForIsolatedMappingTests()
    {
        ExcelRowData row = ExcelReaderMoqExtensions.CreateRow(
            1,
            ("Activity ID", "A-300"),
            ("Activity Name", "Propulsion Alignment"),
            ("WBS Code", "WBS-301"),
            ("Status", "Complete"),
            ("Planned Start", "2026-02-01"),
            ("Planned Finish", "2026-02-10"),
            ("Original Duration (h)", "40"));

        Mock<IExcelWorkbookAccessor> accessorMock = ExcelReaderMoqExtensions.CreateWorkbookAccessorMock("Activities", row);
        var service = new ExcelReaderService(accessorMock.Object);

        ExcelReadResult<PrimaveraActivityRow> result = await service.ReadAsync(
            Stream.Null,
            PrimaveraSheetProfiles.StandardActivityExport);

        accessorMock.Verify(
            accessor => accessor.ReadSheetAsync(
                It.IsAny<Stream>(),
                "Activities",
                0,
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);

        PrimaveraActivityRow activity = Assert.Single(result.Rows);
        Assert.Equal("A-300", activity.ActivityId);
        Assert.Equal("Propulsion Alignment", activity.ActivityName);
    }

    [Fact]
    public async Task ReadAsync_SkipsEmptyRowsAndRecordsReason()
    {
        ExcelRowData validRow = ExcelReaderMoqExtensions.CreateRow(
            1,
            ("Activity ID", "A-400"),
            ("Activity Name", "Tank Testing"),
            ("WBS Code", "WBS-400"),
            ("Status", null),
            ("Planned Start", null),
            ("Planned Finish", null),
            ("Original Duration (h)", null));

        ExcelRowData emptyRow = ExcelReaderMoqExtensions.CreateRow(
            2,
            ("Activity ID", null),
            ("Activity Name", null),
            ("WBS Code", null),
            ("Status", null),
            ("Planned Start", null),
            ("Planned Finish", null),
            ("Original Duration (h)", null));

        Mock<IExcelWorkbookAccessor> accessorMock = ExcelReaderMoqExtensions.CreateWorkbookAccessorMock(
            "Activities",
            validRow,
            emptyRow);

        var service = new ExcelReaderService(accessorMock.Object);

        ExcelReadResult<PrimaveraActivityRow> result = await service.ReadAsync(
            Stream.Null,
            PrimaveraSheetProfiles.StandardActivityExport);

        Assert.Single(result.Rows);
        Assert.Contains(result.SkippedRowReasons, reason => reason.Contains("empty row", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ReadAsync_MapsLegacyPrimaveraTaskSheetLayout()
    {
        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet(
                "TASK",
                [
                    ["task_code", "task_name", "wbs_id", "task_type", "status_code", "early_start_date", "early_end_date", "target_drtn_hr_cnt"],
                    ["A-500", "Steel Cutting", "WBS-500", "TT_Task", "Active", "2026-01-01", "2026-01-05", "32"],
                    ["M-001", "Keel Laying Milestone", "WBS-500", "TT_Mile", "Complete", "2026-01-06", "2026-01-06", "0"]
                ])
            .BuildStream();

        var service = new ExcelReaderService(new NpoiExcelWorkbookAccessor());

        ExcelReadResult<PrimaveraActivityRow> result = await service.ReadAsync(
            stream,
            PrimaveraSheetProfiles.LegacyActivityExport);

        PrimaveraActivityRow activity = Assert.Single(result.Rows);
        Assert.Equal("A-500", activity.ActivityId);
        Assert.Equal("Steel Cutting", activity.ActivityName);
        Assert.DoesNotContain(result.Rows, row => row.ActivityId == "M-001");
    }

    [Fact]
    public async Task ReadAsync_MapsResourceAssignmentSheetByColumnIndex()
    {
        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet(
                "Resource Assignments",
                [
                    ["Assignment ID", "Activity ID", "Resource Description", "Resource ID", "Role ID", "Budgeted Units", "Remaining Units"],
                    ["T-100", "A-100", "Welding crew lead", "RES-WELD-01", "WELD", "160", "120"]
                ])
            .BuildStream();

        var service = new ExcelReaderService(new NpoiExcelWorkbookAccessor());

        ExcelReadResult<PrimaveraTaskRow> result = await service.ReadAsync(
            stream,
            PrimaveraSheetProfiles.ResourceAssignmentExport);

        PrimaveraTaskRow task = Assert.Single(result.Rows);
        Assert.Equal("T-100", task.TaskId);
        Assert.Equal("A-100", task.ActivityId);
        Assert.Equal("Welding crew lead", task.TaskName);
        Assert.Equal("RES-WELD-01", task.ResourceName);
        Assert.Equal("160", task.BudgetedUnits);
        Assert.Equal("WELD", task.TradeCode);
    }
}
