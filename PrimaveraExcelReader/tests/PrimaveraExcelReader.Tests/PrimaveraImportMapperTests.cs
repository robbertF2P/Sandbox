using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Npoi;
using PrimaveraExcelReader.Primavera.Mapping;
using PrimaveraExcelReader.Primavera.Models;
using PrimaveraExcelReader.Primavera.Profiles;

namespace PrimaveraExcelReader.Tests;

public sealed class PrimaveraImportMapperTests
{
    [Fact]
    public void ToActivityImportDto_MapsPrimaveraExternalIds()
    {
        var row = new PrimaveraActivityRow
        {
            ActivityId = "A-100",
            ActivityName = "Block Erection",
            WbsCode = "WBS-204"
        };

        ActivityImportDto dto = PrimaveraImportMapper.ToActivityImportDto(row);

        Assert.Equal("A-100", dto.Id);
        Assert.Equal("Block Erection", dto.Name);
        Assert.Equal("A-100", dto.ExternalIds!["Primavera"]);
        Assert.Equal("WBS-204", dto.ExternalIds["WBS"]);
    }

    [Fact]
    public void ToAssignmentImportDto_ParsesBudgetedHours()
    {
        var row = new PrimaveraTaskRow
        {
            TaskId = "T-200",
            ActivityId = "A-100",
            TaskName = "Structural Welding",
            ResourceName = "Elena Petrov",
            BudgetedUnits = "48.5"
        };

        AssignmentImportDto dto = PrimaveraImportMapper.ToAssignmentImportDto(row);

        Assert.Equal("T-200", dto.Id);
        Assert.Equal("Elena Petrov", dto.PersonName);
        Assert.Equal(48.5m, dto.BudgetedHours);
    }

    [Fact]
    public async Task ReadAndMapWorkflow_ConvertsWorkbookRowsToImportDtos()
    {
        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet(
                "Activities",
                [
                    ["Activity ID", "Activity Name", "WBS Code", "Status", "Planned Start", "Planned Finish", "Original Duration (h)"],
                    ["A-600", "Deck Assembly", "WBS-600", "Active", "2026-04-01", "2026-04-20", "96"]
                ])
            .BuildStream();

        var reader = new ExcelReaderService(new NpoiExcelWorkbookAccessor());

        ExcelReadResult<PrimaveraActivityRow> activities = await reader.ReadAsync(
            stream,
            PrimaveraSheetProfiles.StandardActivityExport);

        await using MemoryStream wbsStream = new ExcelWorkbookTestBuilder()
            .AddSheet(
                "WBS",
                [
                    ["WBS Code", "WBS Name", "Parent WBS Code", "Project ID"],
                    ["WBS-600", "Deck Section", "WBS-200", "HULL-247"]
                ])
            .BuildStream();

        ExcelReadResult<PrimaveraWbsRow> wbsRows = await reader.ReadAsync(
            wbsStream,
            PrimaveraSheetProfiles.StandardWbsExport);

        ActivityImportDto activityDto = PrimaveraImportMapper.ToActivityImportDto(activities.Rows[0]);
        ComponentImportDto componentDto = PrimaveraImportMapper.ToComponentImportDto(wbsRows.Rows[0]);

        Assert.Equal("A-600", activityDto.ExternalIds!["Primavera"]);
        Assert.Equal("WBS-600", componentDto.ExternalIds!["Primavera"]);
        Assert.Equal("Deck Section", componentDto.Name);
    }
}
