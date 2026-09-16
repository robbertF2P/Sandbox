using Akka.Actor;
using Akka.Hosting;
using Akka.Logger.Serilog;
using Common.Utility;
using Common.Utility.Framework;
using Infrastructure.Akka.Actors;
using Infrastructure.Akka.Contracts;
using Infrastructure.Akka.Contracts.Messages;
using Infrastructure.Akka.Facade;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Modularity;

namespace Infrastructure.Akka
{
    [DependsOn(
        typeof(Floor2PlanCommonUtilityModule)
    )]
    public class Floor2PlanInfrastructureAkkaModule : F2PModule
    {
        public const string ActorSystemName = "floor2plan";

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            base.ConfigureServices(context);

            context.Services.AddAkka(ActorSystemName, (builder, serviceProvider) =>
            {
                builder
                    .ConfigureLoggers(setup =>
                    {
                        setup.LogLevel = global::Akka.Event.LogLevel.InfoLevel;
                        setup.AddSerilogLogging();
                    })
                    .WithActors((system, registry) =>
                    {
                        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                        var applicationBridgeProps = ApplicationBridge.Props(scopeFactory);
                        var rootActorRef = system.ActorOf(RootActor.Props(), "root");

                        registry.Register<RootActor>(rootActorRef);
                        rootActorRef.Tell(new InitializeActorSystem(applicationBridgeProps));
                    });
            });

            context.Services.AddSingleton<IActorSystemFacade>(serviceProvider =>
                new ActorSystemFacade(serviceProvider.GetRequiredService<ILogger<ActorSystemFacade>>()));
        }
    }
}
