using Microsoft.Extensions.Hosting;
using Serilog;

namespace Platform.Serilog.Logging;

public static class HostBuilderExtensions
{
    public static IHostBuilder UsePlatformSerilog(
        this IHostBuilder hostBuilder,
        string? applicationName = null)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            SerilogLogging.ConfigureShared(loggerConfiguration, configuration =>
            {
                if (!string.IsNullOrWhiteSpace(applicationName))
                {
                    configuration.Enrich.WithProperty("Application", applicationName);
                }
            });

            SerilogLogging.ConfigureApplicationSinks(
                loggerConfiguration,
                context.Configuration,
                context.HostingEnvironment);
            loggerConfiguration.ReadFrom.Services(services);
        });
    }
}
