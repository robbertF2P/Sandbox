namespace AkkaAspirePoc.Api.Endpoints;

public static class LinksEndpoints
{
    public static IEndpointRouteBuilder MapLinksEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/links", (IConfiguration config, HttpContext http) =>
            Results.Ok(BuildLinks(config, http)))
        .WithTags("Portal");

        app.MapGet("/", (IConfiguration config, HttpContext http) =>
        {
            var acceptsHtml = http.Request.Headers.Accept.Any(h =>
                h?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true);

            if (!acceptsHtml)
            {
                return Results.Ok(new
                {
                    Name = "Akka.NET + Aspire Todo API",
                    Portal = "/",
                    Links = "/api/links",
                    Health = "/health",
                    Todos = "/api/todos"
                });
            }

            var links = BuildLinks(config, http);
            return Results.Content(RenderLandingHtml(links), "text/html");
        })
        .ExcludeFromDescription();

        return app;
    }

    internal static PortalLinksResponse BuildLinks(IConfiguration config, HttpContext http)
    {
        var request = http.Request;
        var apiBase = $"{request.Scheme}://{request.Host}";
        var sentryDsn = config["Sentry:Dsn"];
        var sentryProjectUrl = PortalLinkResolver.SanitizeLinkForPublicRequest(config["Sentry:ProjectUrl"], request);
        var aspireDashboardUrl = PortalLinkResolver.SanitizeLinkForPublicRequest(
            PortalLinkResolver.ResolveAspireDashboardUrl(config),
            request);
        var webBase = ResolveWebBase(config, request, apiBase);
        var aspireAvailable = !string.IsNullOrWhiteSpace(aspireDashboardUrl);
        var aspireStatus = aspireAvailable
            ? null
            : PortalLinkResolver.AspireUnavailableStatus(config);
        var sentryAvailable = !string.IsNullOrWhiteSpace(sentryDsn) && !string.IsNullOrWhiteSpace(sentryProjectUrl);

        return new PortalLinksResponse(
            AspireDashboard: CreateLink(
                "Aspire dashboard",
                aspireDashboardUrl,
                "Distributed app orchestration, logs, traces, and resource health.",
                aspireAvailable,
                aspireStatus),
            Sentry: CreateLink(
                "Sentry performance",
                sentryProjectUrl,
                sentryAvailable
                    ? "View performance traces and errors in your Sentry project."
                    : "Optional cloud observability — no local UI.",
                sentryAvailable,
                sentryAvailable ? null : PortalLinkResolver.SentryUnavailableStatus(config)),
            Api: new ApiLinks(apiBase, $"{apiBase}/health", $"{apiBase}/api/todos", $"{apiBase}/api/links"),
            Web: new WebLinks(webBase, $"{webBase}/todos"));
    }

    private static string ResolveWebBase(IConfiguration config, HttpRequest request, string apiBase)
    {
        var configuredWeb = config["Web:BaseUrl"];
        if (string.IsNullOrWhiteSpace(configuredWeb))
        {
            return apiBase;
        }

        var isLocalRequest = PortalLinkResolver.IsLocalHost(request.Host.Host);
        var isLocalWebConfig = configuredWeb.Contains("localhost", StringComparison.OrdinalIgnoreCase)
            || configuredWeb.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);

        return isLocalWebConfig && !isLocalRequest
            ? apiBase
            : configuredWeb;
    }

    private static PortalLink CreateLink(
        string title,
        string? url,
        string description,
        bool available,
        string? status = null) =>
        new(title, url, description, available, status);

    private static string RenderLandingHtml(PortalLinksResponse links) =>
        $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1" />
          <title>Akka Aspire POC</title>
          <style>
            :root { color-scheme: dark; font-family: Inter, system-ui, sans-serif; }
            body { margin: 0; min-height: 100vh; background: linear-gradient(160deg, #0f172a, #1e293b 45%, #0f766e); color: #e2e8f0; }
            main { max-width: 760px; margin: 0 auto; padding: 3rem 1.5rem; }
            h1 { margin: 0 0 0.5rem; }
            p.lead { color: #94a3b8; margin: 0 0 2rem; }
            .grid { display: grid; gap: 1rem; }
            .card { padding: 1.25rem; border-radius: 0.75rem; background: rgba(15,23,42,.75); border: 1px solid #334155; }
            .card h2 { margin: 0 0 0.5rem; font-size: 1.1rem; }
            .card p { margin: 0 0 1rem; color: #94a3b8; font-size: 0.95rem; }
            a.button { display: inline-block; padding: 0.6rem 1rem; border-radius: 0.5rem; background: #14b8a6; color: #042f2e; text-decoration: none; font-weight: 600; }
            a.button.secondary { background: #334155; color: #e2e8f0; margin-left: 0.5rem; }
            .muted { color: #64748b; font-size: 0.85rem; }
            .disabled { opacity: 0.65; }
          </style>
        </head>
        <body>
          <main>
            <h1>Akka Aspire POC</h1>
            <p class="lead">Portal for the API host. Open the Angular app for the full experience.</p>
            <div class="grid">
              {{RenderCard(links.Web.TodosUrl, "Angular app", "Todo UI and portal home.", true, "Open app")}}
              {{RenderCard(links.AspireDashboard)}}
              {{RenderCard(links.Sentry)}}
              {{RenderCard(links.Api.HealthUrl, "API health", "Liveness and readiness checks.", true, "Health")}}
              {{RenderCard(links.Api.TodosUrl, "Todos API", "REST endpoint backed by Akka actors.", true, "API")}}
            </div>
            <p class="muted" style="margin-top:2rem">JSON links: <a href="{{links.Api.LinksUrl}}" style="color:#5eead4">{{links.Api.LinksUrl}}</a></p>
          </main>
        </body>
        </html>
        """;

    private static string RenderCard(string url, string title, string description, bool available, string? label = null) =>
        RenderCard(new PortalLink(title, url, description, available), label);

    private static string RenderCard(PortalLink link, string? label = null)
    {
        var buttonLabel = label ?? "Open";
        if (!link.Available || string.IsNullOrWhiteSpace(link.Url))
        {
            var status = string.IsNullOrWhiteSpace(link.Status)
                ? "Unavailable — see README."
                : link.Status;

            return $"""
            <section class="card disabled">
              <h2>{link.Title}</h2>
              <p>{link.Description}</p>
              <span class="muted">{status}</span>
            </section>
            """;
        }

        return $"""
        <section class="card">
          <h2>{link.Title}</h2>
          <p>{link.Description}</p>
          <a class="button" href="{link.Url}" target="_blank" rel="noreferrer">{buttonLabel}</a>
        </section>
        """;
    }
}

public sealed record PortalLink(string Title, string? Url, string Description, bool Available, string? Status = null);

public sealed record ApiLinks(string BaseUrl, string HealthUrl, string TodosUrl, string LinksUrl);

public sealed record WebLinks(string BaseUrl, string TodosUrl);

public sealed record PortalLinksResponse(
    PortalLink AspireDashboard,
    PortalLink Sentry,
    ApiLinks Api,
    WebLinks Web);
