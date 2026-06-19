using ApiImportActorPoc.Api.Tests.Excel.Infrastructure;
using ApiImportActorPoc.Core.Excel.Abstractions;
using ApiImportActorPoc.Core.Excel.Mapping;
using ApiImportActorPoc.Core.Excel.Npoi;

namespace ApiImportActorPoc.Api.Tests.Excel;

public sealed class FlexibleColumnMappingTests
{
    [Fact]
    public async Task CustomProfile_MapsAlternateHeadersToSameModel()
    {
        var alternateProfile = new ExcelSheetProfile<ShipyardActivityRow>
        {
            SheetName = "Schedule",
            HeaderRowIndex = 0,
            DataStartRowIndex = 1,
            ColumnBindings =
            [
                new ExcelColumnBinding<ShipyardActivityRow>("Code", (row, value) => row.Code = value ?? string.Empty, required: true),
                new ExcelColumnBinding<ShipyardActivityRow>("Description", (row, value) => row.Description = value ?? string.Empty, required: true),
                new ExcelColumnBinding<ShipyardActivityRow>("Area", (row, value) => row.Area = value ?? string.Empty)
            ]
        };

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
        var profile = new ExcelSheetProfile<ShipyardTaskRow>
        {
            SheetName = "Labor",
            ColumnBindings =
            [
                new ExcelColumnBinding<ShipyardTaskRow>("Task Code", (row, value) => row.TaskCode = value ?? string.Empty, required: true),
                new ExcelColumnBinding<ShipyardTaskRow>("Hours", (row, value) => row.HoursText = value)
            ],
            AfterMap = (model, row) =>
            {
                model.DisplayName = $"{model.TaskCode} ({row.GetByHeader("Trade")})";
                return model;
            }
        };

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
                    new ExcelSheetProfile<ShipyardCombinedRow>
                    {
                        SheetName = "Activities",
                        ColumnBindings =
                        [
                            new ExcelColumnBinding<ShipyardCombinedRow>("Activity ID", (row, value) => row.Key = value ?? string.Empty),
                            new ExcelColumnBinding<ShipyardCombinedRow>("Activity Name", (row, value) => row.Label = value ?? string.Empty)
                        ]
                    }
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
