using F2pPlatform.Host.Hubs;
using F2pPlatform.Host.Services;
using Platform.Serilog.Logging;
using ControlPlane.Api;
using HourApprovals.Api;
using HourApprovals.Packs.Acme;
using Identity.Api;
using Reference.Api;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddPlatformLogging("F2P Platform Host");

    Log.Information("Starting F2P Platform 2.0 host");

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "F2P Platform 2.0 Host",
            Version = "v1",
            Description = "Composed host with Reference module, Akka orchestration, and SignalR events.",
        });
    });
    builder.Services.AddHealthChecks();
    builder.Services.AddSignalR();
    builder.Services.AddF2pPlatformActors();
    builder.Services.AddIdentityModule(builder.Configuration);
    builder.Services.AddReferenceModule(builder.Configuration);
    builder.Services.AddAcmeHourApprovalsPack();
    builder.Services.AddHourApprovalsModule(builder.Configuration);
    builder.Services.AddControlPlaneModule(builder.Configuration);

    var app = builder.Build();

    app.UsePlatformCorrelationPipeline();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "F2P Platform Host v1");
        options.RoutePrefix = "swagger";
    });
    app.UsePlatformRequestLogging();

    app.MapGet("/", () => Results.Ok(new
    {
        Name = "F2P Platform 2.0 Host",
        Swagger = "/swagger",
        IdentityLogin = "/api/identity/login",
        ReferenceStatus = "/api/reference/status",
        AdminTenants = "/admin/tenants",
        Health = "/health",
        SignalRHub = "/hubs/platform-events",
    }));
    app.Lifetime.ApplicationStarted.Register(() =>
        Log.Information("Listening on: {Urls}", string.Join(", ", app.Urls)));
    app.MapHealthChecks("/health");
    app.MapIdentityModule();
    app.MapReferenceModule();
    app.MapHourApprovalsModule();
    app.MapControlPlaneModule();
    app.MapHub<PlatformEventsHub>("/hubs/platform-events");

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "F2P Platform host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
