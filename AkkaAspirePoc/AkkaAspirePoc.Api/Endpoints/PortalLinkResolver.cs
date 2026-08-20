namespace AkkaAspirePoc.Api.Endpoints;

public static class PortalLinkResolver
{
    private const string DefaultAspireDashboardUrl = "https://localhost:17261";

    public static string? ResolveAspireDashboardUrl(IConfiguration config)
    {
        foreach (var candidate in GetAspireDashboardCandidates(config))
        {
            if (IsResolvableUrl(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    public static bool IsRunningUnderAspire(IConfiguration config) =>
        !string.IsNullOrWhiteSpace(config["OTEL_EXPORTER_OTLP_ENDPOINT"])
        || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"))
        || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DOTNET_RESOURCE_SERVICE_ENDPOINT_URL"));

    public static string AspireUnavailableStatus(IConfiguration config) =>
        IsRunningUnderAspire(config)
            ? $"Open the dashboard at {DefaultAspireDashboardUrl} (see AppHost console for the login token)."
            : "Start the AppHost with Docker (`dotnet run --project AkkaAspirePoc.AppHost`).";

    public static string SentryUnavailableStatus(IConfiguration config)
    {
        var hasDsn = !string.IsNullOrWhiteSpace(config["Sentry:Dsn"]);
        var hasProjectUrl = !string.IsNullOrWhiteSpace(config["Sentry:ProjectUrl"]);

        if (hasDsn && !hasProjectUrl)
        {
            return "Optional — set Sentry:ProjectUrl (or sentry-project-url) for a portal link.";
        }

        if (!hasDsn && hasProjectUrl)
        {
            return "Optional — set Sentry:Dsn to send traces and enable the portal link.";
        }

        return "Optional — set Sentry:Dsn and Sentry:ProjectUrl in appsettings or user secrets.";
    }

    private static IEnumerable<string?> GetAspireDashboardCandidates(IConfiguration config)
    {
        yield return config["Aspire:DashboardUrl"];
        yield return Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_URL");

        if (IsRunningUnderAspire(config))
        {
            yield return DefaultAspireDashboardUrl;
        }
    }

    private static bool IsResolvableUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && !value.Contains('{', StringComparison.Ordinal)
        && Uri.TryCreate(value, UriKind.Absolute, out _);
}
