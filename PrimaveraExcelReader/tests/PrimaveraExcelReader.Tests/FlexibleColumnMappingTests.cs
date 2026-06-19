using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Npoi;

namespace PrimaveraExcelReader.Tests;

public sealed class FlexibleColumnMappingTests
{
    [Fact]
    public async Task CustomProfile_MapsAlternateHeadersToSameModel()
    {
        ExcelSheetProfile<ShipyardActivityRow> alternateProfile = ExcelSheetProfile<ShipyardActivityRow>.Configure()
            .Sheet("Schedule")
            .HeaderRow(0)
            .DataStartsAt(1)
            .Map(row => row.Code).From("Code", required: true)
            .Map(row => row.Description).From("Description", required: true)
            .Map(row => row.Area).From("Area")
            .Build();

        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet(
                "Schedule",
                [
                    ["Code", "Description", "Area"],
                    ["ACT-01", "Install propeller shaft", "AFT"]
                ])
            .BuildStream();

        var service = new ExcelReaderService(new NpoiExcelWorkbookAccessor());
        ExcelReadResult<ShipyardActivityRow> result = await service.ReadAsync(stream, alternateProfile);

        ShipyardActivityRow row = Assert.Single(result.Rows);
        Assert.Equal("ACT-01", row.Code);
        Assert.Equal("Install propeller shaft", row.Description);
        Assert.Equal("AFT", row.Area);
    }

    [Fact]
    public async Task CustomProfile_UsesAfterMapForDerivedFields()
    {
        ExcelSheetProfile<ShipyardTaskRow> profile = ExcelSheetProfile<ShipyardTaskRow>.Configure()
            .Sheet("Labor")
            .Map(row => row.TaskCode).From("Task Code", required: true)
            .MapOptional(row => row.HoursText).From("Hours")
            .AfterMap((model, row) =>
            {
                model.DisplayName = $"{model.TaskCode} ({row.GetByHeader("Trade")})";
                return model;
            })
            .Build();

        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet(
                "Labor",
                [
                    ["Task Code", "Hours", "Trade"],
                    ["TSK-01", "12", "PIPE"]
                ])
            .BuildStream();

        var service = new ExcelReaderService(new NpoiExcelWorkbookAccessor());
        ExcelReadResult<ShipyardTaskRow> result = await service.ReadAsync(stream, profile);

        ShipyardTaskRow row = Assert.Single(result.Rows);
        Assert.Equal("TSK-01 (PIPE)", row.DisplayName);
    }

    [Fact]
    public async Task ReadManyAsync_ReadsMultipleSheetsWithDifferentProfiles()
    {
        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet(
                "Activities",
                [
                    ["Activity ID", "Activity Name", "WBS Code", "Status", "Planned Start", "Planned Finish", "Original Duration (h)"],
                    ["A-700", "Launch Preparation", "WBS-700", "Active", "2026-05-01", "2026-05-10", "64"]
                ])
            .AddSheet(
                "Tasks",
                [
                    ["Task ID", "Activity ID", "Task Name", "Resource Name", "Budgeted Units", "Remaining Units", "Trade Code"],
                    ["T-700", "A-700", "Dock checks", "Marco van Berg", "24", "24", "QA"]
                ])
            .BuildStream();

        var service = new ExcelReaderService(new NpoiExcelWorkbookAccessor());

        IReadOnlyDictionary<string, ExcelReadResult<ShipyardCombinedRow>> activityResults =
            await service.ReadManyAsync(
                stream,
                [
                    ExcelSheetProfile<ShipyardCombinedRow>.Configure()
                        .Sheet("Activities")
                        .Map(row => row.Key).From("Activity ID")
                        .Map(row => row.Label).From("Activity Name")
                        .Build()
                ]);

        Assert.True(activityResults.ContainsKey("Activities"));
        Assert.Single(activityResults["Activities"].Rows);
    }

    private sealed class ShipyardActivityRow
    {
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;
    }

    private sealed class ShipyardTaskRow
    {
        public string TaskCode { get; set; } = string.Empty;

        public string? HoursText { get; set; }

        public string DisplayName { get; set; } = string.Empty;
    }

    private sealed class ShipyardCombinedRow
    {
        public string Key { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }
}
