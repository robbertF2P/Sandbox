using Microsoft.AspNetCore.Builder;

namespace Platform.Serilog.Logging.Correlation;

public static class CorrelationApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePlatformCorrelation(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationMiddleware>();
}
