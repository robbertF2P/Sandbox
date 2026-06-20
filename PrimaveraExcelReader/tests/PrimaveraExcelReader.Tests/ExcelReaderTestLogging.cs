using Microsoft.Extensions.Logging;
using Platform.Serilog.Logging.Testing;

namespace PrimaveraExcelReader.Tests;

public static class ExcelReaderTestLogging
{
    public static ILogger<T> CreateLogger<T>() =>
        SerilogTestLogging.CreateLogger<T>(configuration =>
            configuration.Enrich.WithProperty("Application", "PrimaveraExcelReader"));

    public static ILoggerFactory CreateLoggerFactory() =>
        SerilogTestLogging.CreateLoggerFactory(configuration =>
            configuration.Enrich.WithProperty("Application", "PrimaveraExcelReader"));
}
