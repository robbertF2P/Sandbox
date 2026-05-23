using Akka.Hosting;
using AkkaSignalRVuePoc.Api.Actors;
using AkkaSignalRVuePoc.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AkkaSignalRVuePoc.Api.Services;

public static class AkkaHostingExtensions
{
    public static IServiceCollection AddAkkaActors(this IServiceCollection services)
    {
        services.AddAkka<AkkaActorHostedService>("akka-signalr-poc", (builder, sp) =>
        {
            builder
                .ConfigureSerilogLogging()
                .WithActorSystemLivenessCheck()
                .WithActors((system, registry) =>
                {
                    var hubPush = system.ActorOf(
                        SignalRHubPushActor.Props(sp.GetRequiredService<IHubContext<LiveMessagesHub>>()),
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
