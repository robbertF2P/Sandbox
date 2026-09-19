using Akka.Actor;

namespace Infrastructure.Akka.Actors.Guards
{
    /// <summary>
    /// Allows at most one in-flight operation. Rejects or custom-replies when busy.
    /// </summary>
    public sealed class ExclusiveGateActor : ReceiveActor
    {
        private readonly ExclusiveGateOptions _options;
        private bool _busy;

        public ExclusiveGateActor(ExclusiveGateOptions options)
        {
            _options = options;
            Receive<Begin>(HandleBegin);
            Receive<Finished>(_ => _busy = false);
        }

        public static Props Props(ExclusiveGateOptions options)
        {
            return global::Akka.Actor.Props.Create(() => new ExclusiveGateActor(options));
        }

        public sealed record Begin(object Work, IActorRef ReplyTo);

        public sealed record Finished;

        private void HandleBegin(Begin message)
        {
            if (_busy)
            {
                if (_options.BuildRejection != null)
                {
                    message.ReplyTo.Tell(_options.BuildRejection(message.Work));
                }

                return;
            }

            _busy = true;
            _options.OnBegin(message.Work, message.ReplyTo);
        }
    }
}
