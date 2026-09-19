using Akka.Hosting;
using Akka.Logger.Serilog;

namespace Infrastructure.Akka
{
    public static class AkkaSerilogLoggingExtensions
    {
        public static AkkaConfigurationBuilder ConfigureSerilogLogging(this AkkaConfigurationBuilder builder)
        {
            return builder.ConfigureLoggers(setup =>
            {
                setup.LogLevel = global::Akka.Event.LogLevel.InfoLevel;
                setup.ClearLoggers();
                setup.AddSerilogLogging();
                setup.AddLoggerFactory();
            });
        }
    }
}
