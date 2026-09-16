using Akka.Actor;
using Akka.Event;
using Infrastructure.Akka.Contracts.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Akka.Actors
{
    public sealed class ApplicationBridge : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IServiceScopeFactory _scopeFactory;

        public ApplicationBridge(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            Receive<PingApplicationBridge>(HandlePing);
        }

        public static Props Props(IServiceScopeFactory scopeFactory)
        {
            return global::Akka.Actor.Props.Create(() => new ApplicationBridge(scopeFactory));
        }

        private void HandlePing(PingApplicationBridge command)
        {
            using var scope = _scopeFactory.CreateScope();
            _log.Info("ApplicationBridge received ping: {Text}", command.Text);
        }
    }
}
