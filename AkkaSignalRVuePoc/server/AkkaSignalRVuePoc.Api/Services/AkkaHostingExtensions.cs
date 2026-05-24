using Akka.DependencyInjection;
using Akka.Hosting;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Core.Actors;
using AkkaSignalRVuePoc.Core.Publishing;

namespace AkkaSignalRVuePoc.Api.Services;

public static class AkkaHostingExtensions
{
    public static IServiceCollection AddAkkaActors(this IServiceCollection services)
    {
        services.AddSingleton<ILiveMessageClientPublisher, SignalRLiveMessageClientPublisher>();

        services.AddAkka<AkkaActorHostedService>("akka-signalr-poc", (akka, _) =>
        {
            akka
                .ConfigureLoggers(setup =>
                {
                    setup.LogLevel = Akka.Event.LogLevel.InfoLevel;
                    setup.AddLoggerFactory();
                })
                .WithActorSystemLivenessCheck()
                .WithActors((system, registry) =>
                {
                    var resolver = DependencyResolver.For(system);

                    var hubPush = system.ActorOf(
                        resolver.Props<SignalRHubPushActor>(),
                        "signalr-hub-push");

                    var rootActor = system.ActorOf(
                        resolver.Props<LiveMessageRootActor>(hubPush),
                        "live-message-root");
                    registry.Register<LiveMessageRootActor>(rootActor);

                    system.ActorOf(
                        FrontendPushActor.Props(hubPush),
                        "frontend-push");

                    system.EventStream.Publish(new ActorSystemStarted(
                        system.Name,
                        DateTimeOffset.UtcNow));
                });
        });

        return services;
    }
}
