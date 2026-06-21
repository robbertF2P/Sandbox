using Akka.Actor;
using Akka.Hosting;
using F2pPlatform.Host.Contracts.Messages;
using F2pPlatform.Host.Core.Actors;
using F2pPlatform.Host.Core.Publishing;

namespace F2pPlatform.Host.Services;

public static class PlatformAkkaHostingExtensions
{
    public static IServiceCollection AddF2pPlatformActors(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformHubPublisher, SignalRPlatformHubPublisher>();

        services.AddAkka<F2pPlatformActorHostedService>("f2p-platform", (builder, serviceProvider) =>
        {
            builder
                .ConfigureSerilogLogging()
                .WithActorSystemLivenessCheck()
                .WithActors((system, registry) =>
                {
                    IPlatformHubPublisher publisher = serviceProvider.GetRequiredService<IPlatformHubPublisher>();
                    IActorRef signalRHubActor = system.ActorOf(
                        PlatformSignalRHubActor.Props(publisher),
                        "platform-signalr-hub");

                    IActorRef rootActor = system.ActorOf(
                        PlatformRootActor.Props(signalRHubActor),
                        "platform-root");
                    registry.Register<PlatformRootActor>(rootActor);

                    rootActor.Tell(new PublishPlatformStarted(DateTimeOffset.UtcNow));
                });
        });

        return services;
    }
}
