using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Npoi;

namespace PrimaveraExcelReader.Tests;

public sealed class FlexibleColumnMappingTests
{
    [Fact]
    public void TryMapRow_MapsAlternateHeadersToSameModel()
    {
        ExcelSheetProfile<ShipyardActivityRow> profile = ExcelSheetProfile<ShipyardActivityRow>.Configure()
            .Sheet("Schedule")
            .Map(row => row.Code).From("Code", required: true)
            .Map(row => row.Description).From("Description", required: true)
            .Map(row => row.Area).From("Area")
            .Build();

        ExcelRowData row = ExcelRowFactory.FromCells(
            1,
            ("Code", "ACT-01"),
            ("Description", "Install propeller shaft"),
            ("Area", "AFT"));

        ShipyardActivityRow mapped = ExcelReadResultAssert.Success(profile.TryMapRow(row));

        Assert.Equal("ACT-01", mapped.Code);
        Assert.Equal("Install propeller shaft", mapped.Description);
        Assert.Equal("AFT", mapped.Area);
    }

    [Fact]
    public void TryMapRow_UsesAfterMapForDerivedFields()
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

        ExcelRowData row = ExcelRowFactory.FromCells(
            1,
            ("Task Code", "TSK-01"),
            ("Hours", "12"),
            ("Trade", "PIPE"));

        ShipyardTaskRow mapped = ExcelReadResultAssert.Success(profile.TryMapRow(row));

        Assert.Equal("TSK-01 (PIPE)", mapped.DisplayName);
    }

    [Fact]
    public async Task ReadManyAsync_ReadsMultipleSheetsWithDifferentProfiles()
    {
        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet(
                new TestSheetDefinition(
                    "Activities",
                    [
                        PrimaveraSheetScenarios.StandardActivityHeaders,
                        ["A-700", "Launch Preparation", "WBS-700", "Active", "2026-05-01", "2026-05-10", "64"]
                    ]))
            .AddSheet(
                new TestSheetDefinition(
                    "Tasks",
                    [
                        ["Task ID", "Activity ID", "Task Name", "Resource Name", "Budgeted Units", "Remaining Units", "Trade Code"],
                        ["T-700", "A-700", "Dock checks", "Marco van Berg", "24", "24", "QA"]
                    ]))
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
