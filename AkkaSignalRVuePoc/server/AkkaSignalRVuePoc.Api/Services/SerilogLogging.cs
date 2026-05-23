using Serilog;
using Serilog.Events;

namespace AkkaSignalRVuePoc.Api.Services;

public static class SerilogLogging
{
    public static LoggerConfiguration ConfigureShared(LoggerConfiguration configuration)
    {
        return configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Akka", LogEventLevel.Information)
            .Enrich.FromLogContext();
    }

    public static LoggerConfiguration ConfigureApplicationSinks(
        LoggerConfiguration configuration,
        IConfiguration appConfiguration)
    {
        configuration.WriteTo.Console();

        var seqUrl = appConfiguration["Seq:ServerUrl"]
            ?? appConfiguration["Serilog:WriteTo:1:Args:serverUrl"];

        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            configuration.WriteTo.Seq(seqUrl);
        }

        return configuration;
    }

    public static Serilog.ILogger CreateApplicationLogger(IConfiguration appConfiguration, IServiceProvider? services = null)
    {
        var configuration = ConfigureShared(new LoggerConfiguration());
        ConfigureApplicationSinks(configuration, appConfiguration);

        if (services is not null)
        {
            configuration.ReadFrom.Services(services);
        }

        return configuration.CreateLogger();
    }

    public static Serilog.ILogger CreateBootstrapLogger()
    {
        return ConfigureShared(new LoggerConfiguration())
            .WriteTo.Console()
            .CreateBootstrapLogger();
    }
}
