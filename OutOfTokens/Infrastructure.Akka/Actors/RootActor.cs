using Akka.Actor;
using Akka.Event;
using Infrastructure.Akka.Contracts;
using Infrastructure.Akka.Contracts.Messages;

namespace Infrastructure.Akka.Actors
{
    public sealed class RootActor : ReceiveActor
    {
        public const string CustomizeActorName = "customize";
        public const string ApplicationBridgeActorName = "ApplicationBridge";
        public const string FrontDeskActorName = "FrontDesk";

        private readonly ILoggingAdapter _log = Context.GetLogger();

        public RootActor()
        {
            Receive<InitializeActorSystem>(HandleInitialize);
        }

        public static Props Props()
        {
            return global::Akka.Actor.Props.Create(() => new RootActor());
        }

        private void HandleInitialize(InitializeActorSystem command)
        {
            _log.Info("Initializing actor system: creating {0}, {1} and {2} under {3}.",
                ApplicationBridgeActorName, CustomizeActorName, FrontDeskActorName, Self.Path);

            var applicationBridge = Context.ActorOf(command.ApplicationBridgeProps, ApplicationBridgeActorName);
            var customize = Context.ActorOf(CustomizeActor.Props(), CustomizeActorName);
            var frontDesk = Context.ActorOf(FrontDeskActor.Props(applicationBridge), FrontDeskActorName);

            ActorSystemAccess.Initialize(frontDesk, applicationBridge, customize);
            Context.System.EventStream.Publish(new ActorSystemInitCompleted());
        }
    }
}
