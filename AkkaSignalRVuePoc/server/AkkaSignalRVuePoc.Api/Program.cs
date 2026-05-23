using Akka.Actor;
using Akka.Hosting;
using AkkaSignalRVuePoc.Api.Actors;
using AkkaSignalRVuePoc.Api.Hubs;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Akka.NET + SignalR + Vue POC API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    });

    builder.Services.AddSignalR();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("VueDevClient", policy =>
        {
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .GetChildren()
                .Select(origin => origin.Value)
                .OfType<string>()
                .ToArray();

            if (allowedOrigins.Length == 0)
            {
                allowedOrigins = ["http://localhost:5173"];
            }

            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
    builder.Services.AddAkka("akka-signalr-poc", akkaBuilder =>
    {
        akkaBuilder.WithActors((system, registry, resolver) =>
        {
            var hubPush = system.ActorOf(resolver.Props<SignalRHubPushActor>(), "signalr-hub-push");
            registry.Register<SignalRHubPushActor>(hubPush);

            var frontendPush = system.ActorOf(
                Props.Create(() => new FrontendPushActor(hubPush)),
                "frontend-push");
            registry.Register<FrontendPushActor>(frontendPush);
        });
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseCors("VueDevClient");

    app.MapGet("/", () => Results.Ok(new
    {
        Name = "Akka.NET + SignalR + Vue POC",
        Hub = "/hubs/live-messages",
        Message = "Run the Vue client and watch actorMessage events arrive every five seconds."
    }));

    app.MapHub<LiveMessagesHub>("/hubs/live-messages");

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Akka.NET + SignalR + Vue POC API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
