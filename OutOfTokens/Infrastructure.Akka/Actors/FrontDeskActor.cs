using Akka.Actor;
using Akka.Event;
using Infrastructure.Akka.Contracts.Messages;

namespace Infrastructure.Akka.Actors
{
    public sealed class FrontDeskActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IActorRef _applicationBridge;

        public FrontDeskActor(IActorRef applicationBridge)
        {
            _applicationBridge = applicationBridge;

            Receive<PingApplicationBridge>(command =>
            {
                _log.Info("Routing {Command} to ApplicationBridge", nameof(PingApplicationBridge));
                _applicationBridge.Forward(command);
            });
        }

        public static Props Props(IActorRef applicationBridge)
        {
            return global::Akka.Actor.Props.Create(() => new FrontDeskActor(applicationBridge));
        }
    }
}
