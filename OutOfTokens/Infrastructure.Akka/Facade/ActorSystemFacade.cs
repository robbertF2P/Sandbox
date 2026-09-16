using Akka.Actor;
using Infrastructure.Akka.Contracts;
using Infrastructure.Akka.Contracts.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Akka.Facade
{
    public sealed class ActorSystemFacade : IActorSystemFacade
    {
        private static readonly TimeSpan _registerActorTimeout = TimeSpan.FromSeconds(10);

        private readonly ILogger<ActorSystemFacade> _logger;

        public ActorSystemFacade(ILogger<ActorSystemFacade> logger)
        {
            _logger = logger;
        }

        public void Tell<TCommand>(TCommand command) where TCommand : IActorCommand
        {
            var frontDesk = ActorSystemAccess.FrontDesk;
            if (frontDesk == ActorRefs.Nobody)
            {
                _logger.LogWarning("Dropped {Command}: the actor system has not finished initializing yet.", typeof(TCommand).Name);
                return;
            }

            frontDesk.Tell(command);
        }

        public async Task<IActorRef> RegisterActor(string name, Props props)
        {
            var customize = ActorSystemAccess.Customize;
            if (customize == ActorRefs.Nobody)
            {
                _logger.LogWarning("Cannot register actor '{Name}': the actor system has not finished initializing yet.", name);
                return ActorRefs.Nobody;
            }

            var actor = await customize.Ask<IActorRef>(
                new CreateChildActorCommand(name, props),
                _registerActorTimeout,
                CancellationToken.None);

            return actor;
        }
    }
}
