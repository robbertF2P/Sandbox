using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.XUnit3;
using System.Runtime.CompilerServices;

namespace Floor2Plan.TestUtility.Common.Logging
{
    public static class AkkaTestLogging
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            global::Serilog.Log.Logger = CreateLogger();
        }

        public static global::Serilog.ILogger CreateLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Akka", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                .WriteTo.XUnit3TestOutput()
                .CreateLogger();
        }

        public static void AddTestSerilog(this ILoggingBuilder builder, global::Serilog.ILogger logger)
        {
            builder.ClearProviders();
            builder.AddSerilog(logger, dispose: true);
        }
    }
}
