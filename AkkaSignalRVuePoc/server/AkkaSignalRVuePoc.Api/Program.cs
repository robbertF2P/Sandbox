using AkkaSignalRVuePoc.Api.Hubs;
using AkkaSignalRVuePoc.Api.Services;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddHostedService<AkkaActorHostedService>();

var app = builder.Build();

app.UseCors("VueDevClient");

app.MapGet("/", () => Results.Ok(new
{
    Name = "Akka.NET + SignalR + Vue POC",
    Hub = "/hubs/live-messages",
    Message = "Run the Vue client and watch actorMessage events arrive every five seconds."
}));

app.MapHub<LiveMessagesHub>("/hubs/live-messages");

app.Run();
