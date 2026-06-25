using ControlPlane.Api;
using ControlPlane.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Platform.Serilog.Logging;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddPlatformLogging("Admin Backoffice");

    Log.Information("Starting admin backoffice host");

    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:5190"];

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod());
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Admin Backoffice — Control Plane",
            Version = "v1",
            Description = "Platform operator API for tenant provisioning and v2 platform configuration sync.",
        });
    });
    builder.Services.AddHealthChecks();
    builder.Services.AddControlPlaneModule(builder.Configuration);

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ControlPlaneDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    app.UseCors();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "Admin Backoffice v1");
        options.RoutePrefix = "swagger";
    });
    app.UsePlatformRequestLogging();

    app.MapGet("/", () => Results.Ok(new
    {
        Name = "Admin Backoffice — Control Plane",
        Swagger = "/swagger",
        Tenants = "/admin/tenants",
        Health = "/health",
    }));
    app.MapHealthChecks("/health");
    app.MapControlPlaneModule();

    app.Lifetime.ApplicationStarted.Register(() =>
        Log.Information("Listening on: {Urls}", string.Join(", ", app.Urls)));

    await app.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Admin backoffice host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
