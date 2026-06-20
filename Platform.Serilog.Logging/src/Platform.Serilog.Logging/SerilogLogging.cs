using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace Platform.Serilog.Logging;

/// <summary>
/// Platform logging standard:
/// - Development: Console + Datalust Seq
/// - Production: Console + Azure Application Insights
/// - Tests: use <see cref="Testing.SerilogTestLogging"/> (xUnit sink)
/// </summary>
public static class SerilogLogging
{
    public static LoggerConfiguration ConfigureShared(
        LoggerConfiguration configuration,
        Action<LoggerConfiguration>? configure = null)
    {
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Akka", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.With(new Correlation.CorrelationLogEnricher());

        configure?.Invoke(configuration);
        return configuration;
    }

    public static LoggerConfiguration ConfigureApplicationSinks(
        LoggerConfiguration configuration,
        IConfiguration appConfiguration,
        IHostEnvironment environment) =>
        ConfigureApplicationSinks(configuration, appConfiguration, environment.EnvironmentName);

    public static LoggerConfiguration ConfigureApplicationSinks(
        LoggerConfiguration configuration,
        IConfiguration appConfiguration,
        string environmentName)
    {
        configuration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");

        if (IsEnvironment(environmentName, Environments.Development))
        {
            ConfigureSeqSink(configuration, appConfiguration);
        }
        else if (IsEnvironment(environmentName, Environments.Production))
        {
            ConfigureApplicationInsightsSink(configuration, appConfiguration);
        }
        else
        {
            ConfigureSeqSink(configuration, appConfiguration);
            ConfigureApplicationInsightsSink(configuration, appConfiguration);
        }

        return configuration;
    }

    public static ILogger CreateBootstrapLogger(
        IConfiguration? appConfiguration = null,
        string? environmentName = null,
        string? applicationName = null)
    {
        string resolvedEnvironment = environmentName
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environments.Development;

        LoggerConfiguration configuration = ConfigureShared(
            new LoggerConfiguration(),
            enrich =>
            {
                if (!string.IsNullOrWhiteSpace(applicationName))
                {
                    enrich.Enrich.WithProperty("Application", applicationName);
                }
            }).WriteTo.Console();

        if (appConfiguration is not null)
        {
            ConfigureApplicationSinks(configuration, appConfiguration, resolvedEnvironment);
        }

        return configuration.CreateLogger();
    }

    private static bool IsEnvironment(string actual, string expected) =>
        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);

    private static void ConfigureSeqSink(LoggerConfiguration configuration, IConfiguration appConfiguration)
    {
        string? seqUrl = appConfiguration["Seq:ServerUrl"];
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            configuration.WriteTo.Seq(seqUrl);
        }
    }

    private static void ConfigureApplicationInsightsSink(
        LoggerConfiguration configuration,
        IConfiguration appConfiguration)
    {
        string? connectionString = appConfiguration["ApplicationInsights:ConnectionString"]
            ?? appConfiguration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            configuration.WriteTo.ApplicationInsights(connectionString, TelemetryConverter.Traces);
        }
    }
}
