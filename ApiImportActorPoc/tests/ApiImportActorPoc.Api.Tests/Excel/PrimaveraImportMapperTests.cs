using ApiImportActorPoc.Api.Tests.Excel.Infrastructure;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Excel.Mapping;
using ApiImportActorPoc.Core.Excel.Npoi;
using ApiImportActorPoc.Core.Excel.Primavera.Mapping;
using ApiImportActorPoc.Core.Excel.Primavera.Models;
using ApiImportActorPoc.Core.Excel.Primavera.Profiles;

namespace ApiImportActorPoc.Api.Tests.Excel;

public sealed class PrimaveraImportMapperTests
{
    [Fact]
    public void ToActivityImportPayload_MapsPrimaveraExternalIds()
    {
        var row = new PrimaveraActivityRow
        {
            ActivityId = "A-100",
            ActivityName = "Block Erection",
            WbsCode = "WBS-204"
        };

        ActivityImportPayload payload = PrimaveraImportMapper.ToActivityImportPayload(row);

        Assert.Equal("A-100", payload.Id);
        Assert.Equal("Block Erection", payload.Name);
        Assert.Equal("A-100", payload.ExternalIds!["Primavera"]);
        Assert.Equal("WBS-204", payload.ExternalIds["WBS"]);
    }

    [Fact]
    public void ToAssignmentImportPayload_ParsesBudgetedHours()
    {
        var row = new PrimaveraTaskRow
        {
            TaskId = "T-200",
            ActivityId = "A-100",
            TaskName = "Structural Welding",
            ResourceName = "Elena Petrov",
            BudgetedUnits = "48.5"
        };

        AssignmentImportPayload payload = PrimaveraImportMapper.ToAssignmentImportPayload(row);

        Assert.Equal("T-200", payload.Id);
        Assert.Equal("Elena Petrov", payload.PersonName);
        Assert.Equal(48.5m, payload.BudgetedHours);
    }

    [Fact]
    public async Task ReadAndMapWorkflow_ConvertsWorkbookRowsToImportPayloads()
    {
        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet(
                "Activities",
                [
                    ["Activity ID", "Activity Name", "WBS Code", "Status", "Planned Start", "Planned Finish", "Original Duration (h)"],
                    ["A-600", "Deck Assembly", "WBS-600", "Active", "2026-04-01", "2026-04-20", "96"]
                ])
            .AddSheet(
                "WBS",
                [
                    ["WBS Code", "WBS Name", "Parent WBS Code", "Project ID"],
                    ["WBS-600", "Deck Section", "WBS-200", "HULL-247"]
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

        ActivityImportPayload activityPayload = PrimaveraImportMapper.ToActivityImportPayload(activities.Rows[0]);
        ComponentImportPayload componentPayload = PrimaveraImportMapper.ToComponentImportPayload(wbsRows.Rows[0]);

        Assert.Equal("A-600", activityPayload.ExternalIds!["Primavera"]);
        Assert.Equal("WBS-600", componentPayload.ExternalIds!["Primavera"]);
        Assert.Equal("Deck Section", componentPayload.Name);
    }
}
