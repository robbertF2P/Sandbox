using Platform.Serilog.Logging.Testing;
using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Primavera.Models;
using PrimaveraExcelReader.Primavera.Profiles;
using Serilog;

namespace PrimaveraExcelReader.Tests;

public sealed class SerilogLoggingTests
{
    [Fact]
    public async Task ReadRowsAsync_EmitsApplicationStyleLogsToXUnitOutput()
    {
        ExcelRowData row = ExcelRowFactory.FromHeaderRowAndValues(
            1,
            PrimaveraSheetScenarios.StandardActivityHeaders,
            ["A-900", "Log Test Activity", "WBS-900", "Active", "2026-06-01", "2026-06-10", "40"]);

        global::Serilog.Log.Logger = SerilogTestLogging.CreateTestLogger(configuration =>
            configuration.Enrich.WithProperty("Application", "PrimaveraExcelReader"));

        var service = new ExcelReaderService(
            ExcelReaderMoqExtensions.CreateWorkbookAccessorMock(
                PrimaveraSheetProfiles.StandardActivityExport.SheetName,
                row).Object,
            ExcelReaderTestLogging.CreateLogger<ExcelReaderService>());

        ExcelReadResult<PrimaveraActivityRow> result = await service.ReadAsync(
            Stream.Null,
            PrimaveraSheetProfiles.StandardActivityExport,
            TestContext.Current.CancellationToken);

        Assert.Single(result.Rows);
        Assert.Equal("A-900", result.Rows[0].ActivityId);
    }
}
