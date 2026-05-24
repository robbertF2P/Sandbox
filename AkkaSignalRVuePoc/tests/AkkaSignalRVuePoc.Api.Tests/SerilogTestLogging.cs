using AkkaSignalRVuePoc.Api.Services;
using Serilog;
using Serilog.Sinks.XUnit3;

namespace AkkaSignalRVuePoc.Api.Tests;

public static class SerilogTestLogging
{
    public static Serilog.ILogger CreateTestLogger()
    {
        return SerilogLogging.ConfigureShared(new LoggerConfiguration())
            .WriteTo.XUnit3TestOutput()
            .CreateLogger();
    }
}
