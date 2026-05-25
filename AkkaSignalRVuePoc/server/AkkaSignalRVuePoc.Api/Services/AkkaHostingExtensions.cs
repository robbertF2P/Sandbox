using Akka.Actor;
using Akka.Hosting;
using AkkaSignalRVuePoc.Contracts.Interfaces;
using AkkaSignalRVuePoc.Core.Actors;
using AkkaSignalRVuePoc.Core.Publishing;
using AkkaSignalRVuePoc.Data;
using Microsoft.EntityFrameworkCore;

namespace AkkaSignalRVuePoc.Api.Services;

public static class AkkaHostingExtensions
{
    public static IServiceCollection AddAkkaActors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISignalrHubWrapper, SignalRLiveMessageClientPublisher>();
        BackgroundProcessTiming backgroundProcessTiming = ReadBackgroundProcessTiming(configuration);

        services.AddAkka<AkkaActorHostedService>("akka-signalr-poc", (builder, serviceProvider) =>
        {
            builder
                .ConfigureSerilogLogging()
                .WithActorSystemLivenessCheck()
                .WithActors((system, registry) =>
                {
                    IActorRef? hubPush = system.ActorOf(
                        SignalRHubActor.Props(serviceProvider.GetRequiredService<ISignalrHubWrapper>()),
                        "signalr-hub-push");
                    IDbContextFactory<CatalogDbContext> dbContextFactory =
                        serviceProvider.GetRequiredService<IDbContextFactory<CatalogDbContext>>();
                    IActorRef? rootActor = system.ActorOf(
                        RootActor.Props(hubPush, dbContextFactory, backgroundProcessTiming),
                        "live-message-root");
                    registry.Register<RootActor>(rootActor);

                    system.ActorOf(
                        FrontendPushActor.Props(hubPush),
                        "frontend-push");
                });
        });

        services.AddSingleton<IActorSystemCommandFacade>(sp =>
        {
            IActorRef rootActor = sp.GetRequiredService<IRequiredActor<RootActor>>().ActorRef;
            return new ActorSystemCommandFacade(rootActor);
        });

        return services;
    }

    private static BackgroundProcessTiming ReadBackgroundProcessTiming(IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection("BackgroundProcess");
        if (!section.Exists())
        {
            return BackgroundProcessTiming.Default;
        }

        var durationSeconds = section.GetValue<int?>("DurationSeconds");
        var busyIntervalSeconds = section.GetValue<int?>("BusySignalIntervalSeconds");

        if (durationSeconds is null && busyIntervalSeconds is null)
        {
            return BackgroundProcessTiming.Default;
        }

        return new BackgroundProcessTiming(
            TimeSpan.FromSeconds(durationSeconds ?? BackgroundProcessTiming.Default.Duration.TotalSeconds),
            TimeSpan.FromSeconds(
                busyIntervalSeconds ?? BackgroundProcessTiming.Default.BusySignalInterval.TotalSeconds));
    }
}
