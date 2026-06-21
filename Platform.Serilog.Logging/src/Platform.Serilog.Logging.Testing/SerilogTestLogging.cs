using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.XUnit3;

namespace Platform.Serilog.Logging.Testing;

public static class SerilogTestLogging
{
    public static global::Serilog.ILogger CreateTestLogger(Action<LoggerConfiguration>? configure = null)
    {
        LoggerConfiguration loggerConfiguration = SerilogLogging.ConfigureShared(new LoggerConfiguration());
        configure?.Invoke(loggerConfiguration);

        return loggerConfiguration
            .WriteTo.XUnit3TestOutput()
            .CreateLogger();
    }

    public static ILoggerFactory CreateLoggerFactory(Action<LoggerConfiguration>? configure = null)
    {
        global::Serilog.ILogger logger = CreateTestLogger(configure);
        Log.Logger = logger;
        return new SerilogLoggerFactory(logger, dispose: false);
    }

    public static ILogger<T> CreateLogger<T>(Action<LoggerConfiguration>? configure = null) =>
        CreateLoggerFactory(configure).CreateLogger<T>();

    public static void AddPlatformSerilog(this ILoggingBuilder builder, global::Serilog.ILogger logger)
    {
        builder.ClearProviders();
        builder.AddSerilog(logger, dispose: true);
    }
}
