using Akka.Hosting;
using AkkaSignalRVuePoc.Core.Actors;
using AkkaSignalRVuePoc.Core.Publishing;

namespace AkkaSignalRVuePoc.Api.Services;

public static class AkkaHostingExtensions
{
    public static IServiceCollection AddAkkaActors(this IServiceCollection services)
    {
        services.AddSingleton<ISignalrHubWrapper, SignalRLiveMessageClientPublisher>();

        services.AddAkka<AkkaActorHostedService>("akka-signalr-poc", (builder, sp) =>
        {
            builder
                .ConfigureSerilogLogging()
                .WithActorSystemLivenessCheck()
                .WithActors((system, registry) =>
                {
                    var hubPush = system.ActorOf(
                        SignalRHubActor.Props(sp.GetRequiredService<ISignalrHubWrapper>()),
                        "signalr-hub-push");
                    var rootActor = system.ActorOf(LiveMessageRootActor.Props(hubPush), "live-message-root");
                    registry.Register<LiveMessageRootActor>(rootActor);

                    system.ActorOf(
                        FrontendPushActor.Props(hubPush),
                        "frontend-push");
                });
        });

        return services;
    }
}
