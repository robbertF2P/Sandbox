using Akka.Actor;
using Akka.Event;
using Infrastructure.Akka.Contracts.Messages;

namespace Infrastructure.Akka.Actors
{
    public sealed class CustomizeActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public CustomizeActor()
        {
            Receive<CreateChildActorCommand>(command =>
            {
                var child = Context.Child(command.Name);
                if (child == ActorRefs.Nobody)
                {
                    child = Context.ActorOf(command.Props, command.Name);
                    _log.Info("Registered custom actor {ActorPath}", child.Path);
                }

                Sender.Tell(child);
            });
        }

        public static Props Props()
        {
            return global::Akka.Actor.Props.Create(() => new CustomizeActor());
        }
    }
}
