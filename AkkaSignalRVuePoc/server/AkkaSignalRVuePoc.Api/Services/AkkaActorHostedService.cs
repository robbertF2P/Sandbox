using Akka.Actor;
using AkkaSignalRVuePoc.Api.Actors;
using AkkaSignalRVuePoc.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AkkaSignalRVuePoc.Api.Services;

public sealed class AkkaActorHostedService : IHostedService
{
    private readonly IHubContext<LiveMessagesHub> _hubContext;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<AkkaActorHostedService> _logger;
    private ActorSystem? _actorSystem;

    public AkkaActorHostedService(
        IHubContext<LiveMessagesHub> hubContext,
        ILoggerFactory loggerFactory,
        ILogger<AkkaActorHostedService> logger)
    {
        _hubContext = hubContext;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _actorSystem = ActorSystem.Create("akka-signalr-poc");
        _actorSystem.ActorOf(
            Props.Create(() => new FrontendPushActor(
                _hubContext,
                _loggerFactory.CreateLogger<FrontendPushActor>())),
            "frontend-push-actor");

        _logger.LogInformation("Started Akka.NET actor system {ActorSystem}", _actorSystem.Name);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_actorSystem is null)
        {
            return;
        }

        _logger.LogInformation("Stopping Akka.NET actor system {ActorSystem}", _actorSystem.Name);

        try
        {
            await _actorSystem.Terminate().WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Timed out while stopping Akka.NET actor system {ActorSystem}", _actorSystem.Name);
        }
    }
}
