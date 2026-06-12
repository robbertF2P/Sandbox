using ApiImportActorPoc.Api.Endpoints;
using ApiImportActorPoc.Api.Hubs;
using ApiImportActorPoc.Api.Services;
using ApiImportActorPoc.Core.Templates;
using ApiImportActorPoc.Data;
using Serilog;

var bootstrapConfiguration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();
Log.Logger = SerilogLogging.CreateBootstrapLogger(bootstrapConfiguration);

try
{
    Log.Information("Starting API Import Actor POC");

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
            Title = "API Import Actor POC",
            Version = "v1",
            Description = "Import shipbuilding work breakdowns (vessel, blocks, activities) via Akka.NET actors, expose as JSON, persist with EF Core."
        });
    });
    builder.Services.AddImportData(builder.Configuration);
    builder.Services.AddSingleton<ProjectQueryService>();
    builder.Services.AddSingleton<ProgressQueryService>();
    builder.Services.AddSingleton<HourBookingService>();
    builder.Services.AddSingleton<ComponentTemplateService>();
    builder.Services.AddHealthChecks();
    builder.Services.AddAkkaActors();
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
                allowedOrigins = ["http://localhost:5173", "http://localhost:5174"];
            }

            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    var app = builder.Build();

    await ImportDatabaseInitializer.InitializeAsync(app.Services);

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "API Import Actor POC v1");
        options.RoutePrefix = "swagger";
    });

    app.UseSerilogRequestLogging();
    app.UseCors("VueDevClient");

    app.MapGet("/", () => Results.Ok(new
    {
        Name = "Shipbuilding Import Actor POC",
        Swagger = "/swagger",
        Hub = "/hubs/import",
        Import = "/api/import",
        Projects = "/api/projects",
        Assignments = "/api/assignments",
        Health = "/health"
    }));

    app.MapImportEndpoints();
    app.MapProjectEndpoints();
    app.MapProgressEndpoints();
    app.MapAssignmentEndpoints();
    app.MapComponentEndpoints();
    app.MapHealthChecks("/health");
    app.MapHub<ImportHub>("/hubs/import");

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "API Import Actor POC terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
