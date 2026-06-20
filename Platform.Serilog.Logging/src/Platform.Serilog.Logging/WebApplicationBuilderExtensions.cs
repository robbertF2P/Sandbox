using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Platform.Serilog.Logging.Correlation;
using Serilog;

namespace Platform.Serilog.Logging;

public static class WebApplicationBuilderExtensions
{
  /// <summary>
  /// Configures the platform Serilog pipeline on the host and sets the bootstrap logger.
  /// Call before <c>builder.Build()</c>.
  /// </summary>
  public static WebApplicationBuilder AddPlatformLogging(
      this WebApplicationBuilder builder,
      string applicationName)
  {
    Log.Logger = SerilogLogging.CreateBootstrapLogger(
        builder.Configuration,
        builder.Environment.EnvironmentName,
        applicationName);

    builder.Host.UsePlatformSerilog(applicationName);
    return builder;
  }

  public static WebApplication UsePlatformRequestLogging(this WebApplication app)
  {
    app.UseSerilogRequestLogging(options =>
    {
      options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
      {
        if (httpContext.Items.TryGetValue(nameof(CorrelationContext), out object? value)
            && value is CorrelationContext context)
        {
          diagnosticContext.Set("CorrelationId", context.CorrelationId);
          if (!string.IsNullOrWhiteSpace(context.UseCase))
          {
            diagnosticContext.Set("UseCase", context.UseCase);
          }
        }
      };
    });

    return app;
  }

  public static WebApplication UsePlatformCorrelationPipeline(this WebApplication app)
  {
    app.UseMiddleware<CorrelationMiddleware>();
    return app;
  }
}
