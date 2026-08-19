namespace AkkaAspirePoc.Api.Endpoints;

public static class LinksEndpoints
{
    public static IEndpointRouteBuilder MapLinksEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/links", (IConfiguration config, HttpContext http) =>
        {
            var request = http.Request;
            var apiBase = $"{request.Scheme}://{request.Host}";

            var sentryDsn = config["Sentry:Dsn"];
            var sentryProjectUrl = config["Sentry:ProjectUrl"];
            var aspireDashboardUrl = config["Aspire:DashboardUrl"];

            return Results.Ok(new PortalLinksResponse(
                AspireDashboard: CreateLink(
                    "Aspire dashboard",
                    aspireDashboardUrl,
                    "Distributed app orchestration, logs, traces, and resource health.",
                    !string.IsNullOrWhiteSpace(aspireDashboardUrl)),
                Sentry: CreateLink(
                    "Sentry performance",
                    sentryProjectUrl,
                    string.IsNullOrWhiteSpace(sentryDsn)
                        ? "Configure Sentry:Dsn and Sentry:ProjectUrl to send and view traces."
                        : "View performance traces and errors in your Sentry project.",
                    !string.IsNullOrWhiteSpace(sentryDsn) && !string.IsNullOrWhiteSpace(sentryProjectUrl)),
                Api: new ApiLinks(
                    BaseUrl: apiBase,
                    HealthUrl: $"{apiBase}/health",
                    TodosUrl: $"{apiBase}/api/todos",
                    LinksUrl: $"{apiBase}/api/links"),
                Web: new WebLinks(
                    BaseUrl: config["Web:BaseUrl"] ?? "http://localhost:4200",
                    TodosUrl: $"{config["Web:BaseUrl"] ?? "http://localhost:4200"}/todos")));
        })
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
        var sentryProjectUrl = config["Sentry:ProjectUrl"];
        var aspireDashboardUrl = config["Aspire:DashboardUrl"];
        var webBase = config["Web:BaseUrl"] ?? "http://localhost:4200";

        return new PortalLinksResponse(
            AspireDashboard: CreateLink(
                "Aspire dashboard",
                aspireDashboardUrl,
                "Distributed app orchestration, logs, traces, and resource health.",
                !string.IsNullOrWhiteSpace(aspireDashboardUrl)),
            Sentry: CreateLink(
                "Sentry performance",
                sentryProjectUrl,
                string.IsNullOrWhiteSpace(sentryDsn)
                    ? "Configure Sentry:Dsn and Sentry:ProjectUrl to send and view traces."
                    : "View performance traces and errors in your Sentry project.",
                !string.IsNullOrWhiteSpace(sentryDsn) && !string.IsNullOrWhiteSpace(sentryProjectUrl)),
            Api: new ApiLinks(apiBase, $"{apiBase}/health", $"{apiBase}/api/todos", $"{apiBase}/api/links"),
            Web: new WebLinks(webBase, $"{webBase}/todos"));
    }

    private static PortalLink CreateLink(string title, string? url, string description, bool available) =>
        new(title, url, description, available);

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
              {{RenderCard(links.Web.BaseUrl + "/", "Angular app", "Todo UI and portal home.", true, "Open app")}}
              {{RenderCard(links.AspireDashboard.Url, links.AspireDashboard.Title, links.AspireDashboard.Description, links.AspireDashboard.Available)}}
              {{RenderCard(links.Sentry.Url, links.Sentry.Title, links.Sentry.Description, links.Sentry.Available)}}
              {{RenderCard(links.Api.HealthUrl, "API health", "Liveness and readiness checks.", true, "Health")}}
              {{RenderCard(links.Api.TodosUrl, "Todos API", "REST endpoint backed by Akka actors.", true, "API")}}
            </div>
            <p class="muted" style="margin-top:2rem">JSON links: <a href="{{links.Api.LinksUrl}}" style="color:#5eead4">{{links.Api.LinksUrl}}</a></p>
          </main>
        </body>
        </html>
        """;

    private static string RenderCard(string? url, string title, string description, bool available, string? label = null)
    {
        var buttonLabel = label ?? "Open";
        if (!available || string.IsNullOrWhiteSpace(url))
        {
            return $"""
            <section class="card disabled">
              <h2>{title}</h2>
              <p>{description}</p>
              <span class="muted">Not configured — see README.</span>
            </section>
            """;
        }

        return $"""
        <section class="card">
          <h2>{title}</h2>
          <p>{description}</p>
          <a class="button" href="{url}" target="_blank" rel="noreferrer">{buttonLabel}</a>
        </section>
        """;
    }
}

public sealed record PortalLink(string Title, string? Url, string Description, bool Available);

public sealed record ApiLinks(string BaseUrl, string HealthUrl, string TodosUrl, string LinksUrl);

public sealed record WebLinks(string BaseUrl, string TodosUrl);

public sealed record PortalLinksResponse(
    PortalLink AspireDashboard,
    PortalLink Sentry,
    ApiLinks Api,
    WebLinks Web);
