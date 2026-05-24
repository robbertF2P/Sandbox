using Akka.Hosting;
using Akka.Logger.Serilog;

namespace AkkaSignalRVuePoc.Api.Services;

public static class AkkaLoggerConfiguration
{
    public static AkkaConfigurationBuilder ConfigureSerilogLogging(this AkkaConfigurationBuilder builder)
    {
        return builder.ConfigureLoggers(setup =>
        {
            setup.LogLevel = Akka.Event.LogLevel.InfoLevel;
            setup.AddSerilogLogging();
        });
    }
}
