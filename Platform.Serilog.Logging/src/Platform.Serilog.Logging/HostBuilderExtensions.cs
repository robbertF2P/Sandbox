using Microsoft.Extensions.Hosting;
using Serilog;

namespace Platform.Serilog.Logging;

public static class HostBuilderExtensions
{
    public static IHostBuilder UsePlatformSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            SerilogLogging.ConfigureShared(loggerConfiguration);
            SerilogLogging.ConfigureApplicationSinks(
                loggerConfiguration,
                context.Configuration,
                context.HostingEnvironment);
            loggerConfiguration.ReadFrom.Services(services);
        });
    }
}
