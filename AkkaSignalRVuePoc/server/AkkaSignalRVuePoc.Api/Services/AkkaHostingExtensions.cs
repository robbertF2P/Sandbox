using Akka.Actor;
using Akka.Hosting;
using AkkaSignalRVuePoc.Core.Actors;
using AkkaSignalRVuePoc.Core.Publishing;

namespace AkkaSignalRVuePoc.Api.Services;

public static class AkkaHostingExtensions
{
    public static IServiceCollection AddAkkaActors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISignalrHubWrapper, SignalRLiveMessageClientPublisher>();
        var backgroundProcessTiming = ReadBackgroundProcessTiming(configuration);

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
                    var rootActor = system.ActorOf(
                        LiveMessageRootActor.Props(hubPush, backgroundProcessTiming),
                        "live-message-root");
                    registry.Register<LiveMessageRootActor>(rootActor);

                    system.ActorOf(
                        FrontendPushActor.Props(hubPush),
                        "frontend-push");
                });
        });

        services.AddSingleton<IActorSystemCommandFacade>(sp =>
        {
            var rootActor = sp.GetRequiredService<IRequiredActor<LiveMessageRootActor>>().ActorRef;
            return new ActorSystemCommandFacade(rootActor);
        });

        return services;
    }

    private static BackgroundProcessTiming ReadBackgroundProcessTiming(IConfiguration configuration)
    {
        var section = configuration.GetSection("BackgroundProcess");
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
            Duration: TimeSpan.FromSeconds(durationSeconds ?? BackgroundProcessTiming.Default.Duration.TotalSeconds),
            BusySignalInterval: TimeSpan.FromSeconds(
                busyIntervalSeconds ?? BackgroundProcessTiming.Default.BusySignalInterval.TotalSeconds));
    }
}
