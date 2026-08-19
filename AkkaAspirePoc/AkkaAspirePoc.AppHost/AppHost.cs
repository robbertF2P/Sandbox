using Aaron.Akka.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("sql-password", secret: true);

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
    .WaitFor(todosDb)
    .WaitFor(redis);

var web = builder.AddJavaScriptApp("web", "../web", "start")
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithReference(api)
    .WithEnvironment("API_URL", api.GetEndpoint("http"))
    .WithExternalHttpEndpoints();

builder.Build().Run();
