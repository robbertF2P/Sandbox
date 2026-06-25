using Akka.Actor;
using Akka.Hosting;
using F2pPlatform.Host.Contracts.Messages;
using F2pPlatform.Host.Core.Actors;
using F2pPlatform.Host.Core.ApprovalQueue;
using F2pPlatform.Host.Core.ApprovalQueue.Actors;
using F2pPlatform.Host.Core.Publishing;
using Microsoft.Extensions.DependencyInjection;

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

                    IServiceScopeFactory scopeFactory =
                        serviceProvider.GetRequiredService<IServiceScopeFactory>();

                    IActorRef planningReadActor = system.ActorOf(
                        PlanningApprovalReadActor.Props(),
                        "planning-approval-read");
                    registry.Register<PlanningApprovalReadActor>(planningReadActor);

                    IActorRef timekeepingReadActor = system.ActorOf(
                        TimekeepingApprovalReadActor.Props(),
                        "timekeeping-approval-read");
                    registry.Register<TimekeepingApprovalReadActor>(timekeepingReadActor);

                    IActorRef hoursReadActor = system.ActorOf(
                        HoursApprovalReadActor.Props(scopeFactory),
                        "hours-approval-read");
                    registry.Register<HoursApprovalReadActor>(hoursReadActor);

                    rootActor.Tell(new PublishPlatformStarted(DateTimeOffset.UtcNow));
                });
        });

        services.AddSingleton<IApprovalQueueFacade>(sp => new ApprovalQueueFacade(
            sp.GetRequiredService<IRequiredActor<PlanningApprovalReadActor>>().ActorRef,
            sp.GetRequiredService<IRequiredActor<TimekeepingApprovalReadActor>>().ActorRef,
            sp.GetRequiredService<IRequiredActor<HoursApprovalReadActor>>().ActorRef));

        return services;
    }
}
