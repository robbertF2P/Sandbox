using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace PrimaveraExcelReader.Tests;

public static class ExcelReaderTestLogging
{
    public static ILogger<T> CreateLogger<T>()
    {
        return CreateLoggerFactory().CreateLogger<T>();
    }

    public static ILoggerFactory CreateLoggerFactory()
    {
        Log.Logger ??= SerilogTestLogging.CreateTestLogger();

        return new SerilogLoggerFactory(Log.Logger, dispose: false);
    }
}
