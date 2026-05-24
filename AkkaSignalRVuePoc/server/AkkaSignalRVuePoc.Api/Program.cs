using AkkaSignalRVuePoc.Api.Endpoints;
using AkkaSignalRVuePoc.Api.Hubs;
using AkkaSignalRVuePoc.Api.Services;
using Serilog;

Log.Logger = SerilogLogging.CreateBootstrapLogger();

try
{
    Log.Information("Starting Akka.NET + SignalR + Vue POC API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        SerilogLogging.ConfigureShared(loggerConfiguration);
        SerilogLogging.ConfigureApplicationSinks(loggerConfiguration, context.Configuration);
        loggerConfiguration.ReadFrom.Services(services);
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Akka SignalR Vue POC API",
            Version = "v1",
            Description = "Sample REST API for organisations and projects, plus SignalR live messages."
        });
    });
    builder.Services.AddSingleton<InMemoryCatalogStore>();
    builder.Services.AddHealthChecks();
    builder.Services.AddAkkaActors(builder.Configuration);
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

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "Akka SignalR Vue POC API v1");
        options.RoutePrefix = "swagger";
    });

    app.UseSerilogRequestLogging();
    app.UseCors("VueDevClient");

    app.MapGet("/", () => Results.Ok(new
    {
        Name = "Akka.NET + SignalR + Vue POC",
        Swagger = "/swagger",
        Hub = "/hubs/live-messages",
        Organisations = "/api/organisations",
        Projects = "/api/projects",
        Messages = "/api/messages",
        BackgroundProcess = "/api/background-process/start",
        Health = "/health",
        Message = "Open Swagger to explore the REST API, or run the Vue client for live actor messages."
    }));

    app.MapOrganisationEndpoints();
    app.MapProjectEndpoints();
    app.MapMessageEndpoints();
    app.MapBackgroundProcessEndpoints();
    app.MapHealthChecks("/health");
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
