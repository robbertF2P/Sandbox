using Akka.Actor;
using Akka.Hosting;
using ApiImportActorPoc.Contracts.Interfaces;
using ApiImportActorPoc.Core.Actors;
using ApiImportActorPoc.Core.Publishing;
using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Api.Services;

public static class AkkaHostingExtensions
{
    public static IServiceCollection AddAkkaActors(this IServiceCollection services)
    {
        services.AddSingleton<ISignalrHubWrapper, SignalRImportHubPublisher>();

        services.AddAkka<AkkaActorHostedService>("api-import-actor-poc", (builder, serviceProvider) =>
        {
            builder
                .ConfigureSerilogLogging()
                .WithActorSystemLivenessCheck()
                .WithActors((system, registry) =>
                {
                    system.ActorOf(
                        SignalRHubActor.Props(serviceProvider.GetRequiredService<ISignalrHubWrapper>()),
                        "signalr-hub-push");

                    IDbContextFactory<ImportDbContext> dbContextFactory =
                        serviceProvider.GetRequiredService<IDbContextFactory<ImportDbContext>>();
                    IActorRef rootActor = system.ActorOf(
                        RootActor.Props(dbContextFactory),
                        "import-root");
                    registry.Register<RootActor>(rootActor);
                });
        });

        services.AddSingleton<IActorSystemCommandFacade>(sp =>
        {
            IActorRef rootActor = sp.GetRequiredService<IRequiredActor<RootActor>>().ActorRef;
            return new ActorSystemCommandFacade(rootActor);
        });

        return services;
    }
}
