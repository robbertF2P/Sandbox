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
                return ToLoginUrl(candidate, config);
            }
        }

        return null;
    }

    public static string? ToLoginUrl(string? baseUrl, IConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        var token = ResolveDashboardLoginToken(config);
        if (string.IsNullOrWhiteSpace(token))
        {
            return baseUrl.TrimEnd('/');
        }

        var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{baseUrl.TrimEnd('/')}/login{separator}t={token}";
    }

    private static string? ResolveDashboardLoginToken(IConfiguration config)
    {
        var configured = config["Aspire:DashboardLoginToken"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        var fromEnv = Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_LOGIN_TOKEN");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        try
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".aspire",
                "logs");
            if (!Directory.Exists(logDir))
            {
                return null;
            }

            var latest = Directory.GetFiles(logDir, "cli_*.log")
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();

            if (latest is null)
            {
                return null;
            }

            var match = System.Text.RegularExpressions.Regex.Match(
                File.ReadAllText(latest),
                @"login\?t=([a-f0-9]+)");
            return match.Success ? match.Groups[1].Value : null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    public static bool IsRunningUnderAspire(IConfiguration config) =>
        !string.IsNullOrWhiteSpace(config["OTEL_EXPORTER_OTLP_ENDPOINT"])
        || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"))
        || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DOTNET_RESOURCE_SERVICE_ENDPOINT_URL"));

    public static bool IsLocalHost(string? host) =>
        string.IsNullOrWhiteSpace(host)
        || host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Drops localhost-only URLs when the incoming request came from a public host (tunnel, ngrok, etc.).
    /// </summary>
    public static string? SanitizeLinkForPublicRequest(string? url, HttpRequest request)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return url;
        }

        return !IsLocalHost(request.Host.Host) && IsLocalHost(uri.Host) ? null : url;
    }

    public static string AspireUnavailableStatus(IConfiguration config) =>
        IsRunningUnderAspire(config)
            ? "Start cloudflared for the dashboard: cloudflared tunnel --url https://127.0.0.1:17261 --no-tls-verify"
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
        yield return config["Aspire:DashboardPublicUrl"];
        yield return Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_PUBLIC_URL");
        yield return ReadPublicDashboardUrlFromFile();
        yield return config["Aspire:DashboardUrl"];
        yield return Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_URL");

        if (IsRunningUnderAspire(config))
        {
            yield return DefaultAspireDashboardUrl;
        }
    }

    private static string? ReadPublicDashboardUrlFromFile()
    {
        var publicUrlFile = Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_PUBLIC_URL_FILE")
            ?? "/tmp/aspire-dashboard-public-url.txt";

        try
        {
            return File.ReadAllText(publicUrlFile).Trim();
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static bool IsResolvableUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && !value.Contains('{', StringComparison.Ordinal)
        && Uri.TryCreate(value, UriKind.Absolute, out _);
}
