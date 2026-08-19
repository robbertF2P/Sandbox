using AkkaAspirePoc.Api.Actors;
using AkkaAspirePoc.Api.Endpoints;
using AkkaAspirePoc.Data;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Sentry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.AddServiceDefaults();

builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.TracesSampleRate = builder.Configuration.GetValue("Sentry:TracesSampleRate", 1.0);
    options.SendDefaultPii = false;
    options.Debug = builder.Environment.IsDevelopment();
});

var useSqlite = builder.Configuration.GetValue("Demo:UseSqlite", false);
if (useSqlite)
{
    var sqlitePath = builder.Configuration["Demo:SqlitePath"] ?? "demo.db";
    builder.Services.AddDbContext<TodoDbContext>(options =>
        options.UseSqlite($"Data Source={sqlitePath}"));
}
else
{
    builder.AddSqlServerDbContext<TodoDbContext>("todosdb");
}

builder.Services.AddTodoActors(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
    {
        if (useSqlite)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy
            .WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:4200"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseSentryTracing();
app.UseCors("AngularDev");
app.MapDefaultEndpoints();

var webRoot = builder.Configuration["Demo:WebRoot"];
string? resolvedWebRoot = null;
if (!string.IsNullOrWhiteSpace(webRoot))
{
    if (!Path.IsPathRooted(webRoot))
    {
        webRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, webRoot));
    }

    if (Directory.Exists(webRoot))
    {
        resolvedWebRoot = webRoot;
        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webRoot)
        });
    }
}

app.MapLinksEndpoints();
app.MapTodoEndpoints();

if (resolvedWebRoot is not null)
{
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(resolvedWebRoot)
    });
}

await TodoDatabaseInitializer.InitializeAsync(app.Services);

app.Run();

public partial class Program;
