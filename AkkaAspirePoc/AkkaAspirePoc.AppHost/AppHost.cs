using Aaron.Akka.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("sql-password", secret: true);

// Literal URL — AddParameter resolves to a placeholder in child env vars, so the portal never saw a real link.
const string aspireDashboardUrl = "https://localhost:17261";
var sentryProjectUrl = builder.AddParameter("sentry-project-url", "");

var sql = builder.AddSqlServer("sql", password)
    .WithImageTag("2022-latest")
    .WithLifetime(ContainerLifetime.Persistent);

var todosDb = sql.AddDatabase("todosdb");

var redis = builder.AddRedis("akka-discovery")
    .WithLifetime(ContainerLifetime.Persistent);

var akka = builder.AddAkka("todo-cluster")
    .WithClustering(redis);

var api = builder.AddProject<Projects.AkkaAspirePoc_Api>("api")
    .WithHttpEndpoint(name: "http", port: 5080)
    .WithReference(todosDb)
    .WithReference(redis)
    .WithReference(akka)
    .WithEnvironment("Aspire__DashboardUrl", aspireDashboardUrl)
    .WithEnvironment("Sentry__ProjectUrl", sentryProjectUrl)
    .WithEnvironment("Web__BaseUrl", "http://localhost:4200")
    .WaitFor(todosDb)
    .WaitFor(redis);

var web = builder.AddJavaScriptApp("web", "../web", "start")
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithReference(api)
    .WithEnvironment("ASPIRE_DASHBOARD_URL", aspireDashboardUrl)
    .WithEnvironment("SENTRY_PROJECT_URL", sentryProjectUrl)
    .WithExternalHttpEndpoints();

builder.Build().Run();
