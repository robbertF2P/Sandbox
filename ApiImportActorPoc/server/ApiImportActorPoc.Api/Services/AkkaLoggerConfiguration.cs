using Akka.Hosting;
using Akka.Logger.Serilog;

namespace ApiImportActorPoc.Api.Services;

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
