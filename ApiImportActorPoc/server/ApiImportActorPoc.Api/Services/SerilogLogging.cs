using Serilog;
using Serilog.Events;

namespace ApiImportActorPoc.Api.Services;

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

        var seqUrl = appConfiguration["Seq:ServerUrl"];
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            configuration.WriteTo.Seq(seqUrl);
        }

        return configuration;
    }

    public static Serilog.ILogger CreateBootstrapLogger(IConfiguration? configuration = null)
    {
        var loggerConfiguration = ConfigureShared(new LoggerConfiguration())
            .WriteTo.Console();

        var seqUrl = configuration?["Seq:ServerUrl"];
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            loggerConfiguration.WriteTo.Seq(seqUrl);
        }

        return loggerConfiguration.CreateBootstrapLogger();
    }
}
