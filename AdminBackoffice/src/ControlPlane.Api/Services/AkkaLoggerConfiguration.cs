using Akka.Hosting;
using Akka.Logger.Serilog;

namespace ControlPlane.Api.Services;

internal static class AkkaLoggerConfiguration
{
    public static AkkaConfigurationBuilder ConfigureSerilogLogging(this AkkaConfigurationBuilder builder) =>
        builder.ConfigureLoggers(setup =>
        {
            setup.LogLevel = Akka.Event.LogLevel.InfoLevel;
            setup.AddSerilogLogging();
        });
}
