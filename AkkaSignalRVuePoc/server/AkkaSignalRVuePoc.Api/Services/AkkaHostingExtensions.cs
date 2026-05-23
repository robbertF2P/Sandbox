using Akka.DependencyInjection;
using Akka.Hosting;
using AkkaSignalRVuePoc.Api.Actors;

namespace AkkaSignalRVuePoc.Api.Services;

public static class AkkaHostingExtensions
{
    public static IServiceCollection AddAkkaActors(this IServiceCollection services)
    {
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
                });
        });

        return services;
    }
}
