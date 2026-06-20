using Serilog;
using Serilog.Events;

namespace PrimaveraExcelReader.Logging;

public static class SerilogLogging
{
    public static LoggerConfiguration ConfigureShared(LoggerConfiguration configuration)
    {
        return configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "PrimaveraExcelReader");
    }

    public static LoggerConfiguration ConfigureApplicationSinks(LoggerConfiguration configuration)
    {
        return configuration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
    }

    public static ILogger CreateApplicationLogger()
    {
        return ConfigureApplicationSinks(ConfigureShared(new LoggerConfiguration()))
            .CreateLogger();
    }

    public static ILogger CreateBootstrapLogger()
    {
        return ConfigureShared(new LoggerConfiguration())
            .WriteTo.Console()
            .CreateLogger();
    }
}
