using PrimaveraExcelReader.Logging;
using Serilog;
using Serilog.Sinks.XUnit3;

namespace PrimaveraExcelReader.Tests;

public static class SerilogTestLogging
{
    public static Serilog.ILogger CreateTestLogger()
    {
        return SerilogLogging.ConfigureShared(new LoggerConfiguration())
            .WriteTo.XUnit3TestOutput()
            .CreateLogger();
    }
}
