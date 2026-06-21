using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
    app.UseSerilogRequestLogging();
    return app;
  }
}
