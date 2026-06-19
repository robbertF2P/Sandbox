using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Primavera.Mapping;
using PrimaveraExcelReader.Primavera.Models;
using PrimaveraExcelReader.Primavera.Profiles;

namespace PrimaveraExcelReader.Tests;

public sealed class PrimaveraImportMapperTests
{
    public static TheoryData<PrimaveraActivityRow, string, string> ActivityExternalIdCases =>
        new()
        {
            {
                new PrimaveraActivityRow { ActivityId = "A-100", ActivityName = "Block Erection", WbsCode = "WBS-204" },
                "A-100",
                "WBS-204"
            }
        };

    [Theory]
    [MemberData(nameof(ActivityExternalIdCases))]
    public void ToActivityImportDto_MapsPrimaveraExternalIds(
        PrimaveraActivityRow row,
        string expectedPrimaveraId,
        string expectedWbsId)
    {
        ActivityImportDto dto = PrimaveraImportMapper.ToActivityImportDto(row);

        Assert.Equal(expectedPrimaveraId, dto.Id);
        Assert.Equal(row.ActivityName, dto.Name);
        Assert.Equal(expectedPrimaveraId, dto.ExternalIds!["Primavera"]);
        Assert.Equal(expectedWbsId, dto.ExternalIds["WBS"]);
    }

    [Fact]
    public void ToAssignmentImportDto_MapsBudgetedHours()
    {
        var row = new PrimaveraTaskRow
        {
            TaskId = "T-200",
            ActivityId = "A-100",
            TaskName = "Structural Welding",
            ResourceName = "Elena Petrov",
            BudgetedUnits = 48.5m
        };

        AssignmentImportDto dto = PrimaveraImportMapper.ToAssignmentImportDto(row);

        Assert.Equal("T-200", dto.Id);
        Assert.Equal("Elena Petrov", dto.PersonName);
        Assert.Equal(48.5m, dto.BudgetedHours);
    }

    [Fact]
    public void ReadMapImport_ComposesPurePipelineFromRowsToImportDtos()
    {
        ExcelRowData activityRow = ExcelRowFactory.FromHeaderRowAndValues(
            1,
            PrimaveraSheetScenarios.StandardActivityHeaders,
            ["A-600", "Deck Assembly", "WBS-600", "Active", "2026-04-01", "2026-04-20", "96"]);

        ExcelRowData wbsRow = ExcelRowFactory.FromCells(
            1,
            ("WBS Code", "WBS-600"),
            ("WBS Name", "Deck Section"),
            ("Parent WBS Code", "WBS-200"),
            ("Project ID", "HULL-247"));

        (IReadOnlyList<PrimaveraActivityRow> activities, _) =
            RowMappingPipeline.MapRows(PrimaveraSheetProfiles.StandardActivityExport, [activityRow]);

        (IReadOnlyList<PrimaveraWbsRow> wbsRows, _) =
            RowMappingPipeline.MapRows(PrimaveraSheetProfiles.StandardWbsExport, [wbsRow]);

        ActivityImportDto activityDto = PrimaveraImportMapper.ToActivityImportDto(activities[0]);
        ComponentImportDto componentDto = PrimaveraImportMapper.ToComponentImportDto(wbsRows[0]);

        Assert.Equal("A-600", activityDto.ExternalIds!["Primavera"]);
        Assert.Equal("WBS-600", componentDto.ExternalIds!["Primavera"]);
        Assert.Equal("Deck Section", componentDto.Name);
    }

    [Fact]
    public async Task ReadAndMapWorkflow_ConvertsWorkbookRowsToImportDtos()
    {
        ExcelReadResult<PrimaveraActivityRow> activities = await ExcelReaderTestRunner.ReadSheetAsync(
            new TestSheetDefinition(
                "Activities",
                [
                    PrimaveraSheetScenarios.StandardActivityHeaders,
                    ["A-600", "Deck Assembly", "WBS-600", "Active", "2026-04-01", "2026-04-20", "96"]
                ]),
            PrimaveraSheetProfiles.StandardActivityExport);

        ExcelReadResult<PrimaveraWbsRow> wbsRows = await ExcelReaderTestRunner.ReadSheetAsync(
            new TestSheetDefinition(
                "WBS",
                [
                    ["WBS Code", "WBS Name", "Parent WBS Code", "Project ID"],
                    ["WBS-600", "Deck Section", "WBS-200", "HULL-247"]
                ]),
            PrimaveraSheetProfiles.StandardWbsExport);

        ActivityImportDto activityDto = PrimaveraImportMapper.ToActivityImportDto(activities.Rows[0]);
        ComponentImportDto componentDto = PrimaveraImportMapper.ToComponentImportDto(wbsRows.Rows[0]);

        Assert.Equal("A-600", activityDto.ExternalIds!["Primavera"]);
        Assert.Equal("WBS-600", componentDto.ExternalIds!["Primavera"]);
    }
}
