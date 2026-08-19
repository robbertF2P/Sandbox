using AkkaAspirePoc.Api.Actors;
using AkkaAspirePoc.Api.Endpoints;
using AkkaAspirePoc.Data;
using Sentry;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.TracesSampleRate = builder.Configuration.GetValue("Sentry:TracesSampleRate", 1.0);
    options.SendDefaultPii = false;
    options.Debug = builder.Environment.IsDevelopment();
});

builder.AddSqlServerDbContext<TodoDbContext>("todosdb");

builder.Services.AddTodoActors();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
    {
        policy
            .WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:4200"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSentryTracing();
app.UseCors("AngularDev");
app.MapDefaultEndpoints();

app.MapLinksEndpoints();
app.MapTodoEndpoints();

await TodoDatabaseInitializer.InitializeAsync(app.Services);

app.Run();

public partial class Program;
