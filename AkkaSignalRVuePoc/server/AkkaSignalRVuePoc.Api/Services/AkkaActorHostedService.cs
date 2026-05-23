using Akka.Actor;
using Akka.DependencyInjection;
using AkkaSignalRVuePoc.Api.Actors;

namespace AkkaSignalRVuePoc.Api.Services;

public sealed class AkkaActorHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AkkaActorHostedService> _logger;
    private ActorSystem? _actorSystem;

    public AkkaActorHostedService(
        IServiceProvider serviceProvider,
        ILogger<AkkaActorHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _actorSystem = ActorSystem.Create(
            "akka-signalr-poc",
            BootstrapSetup.Create().And(DependencyResolverSetup.Create(_serviceProvider)));

        var resolver = DependencyResolver.For(_actorSystem);
        var hubPush = _actorSystem.ActorOf(
            resolver.Props<SignalRHubPushActor>(),
            "signalr-hub-push");

        _actorSystem.ActorOf(
            Props.Create(() => new FrontendPushActor(hubPush)),
            "frontend-push");

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
            _logger.LogWarning(
                "Timed out while stopping Akka.NET actor system {ActorSystem}",
                _actorSystem.Name);
        }
    }
}
