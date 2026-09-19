using Akka.Actor;

namespace Infrastructure.Akka.Actors.Workers
{
    /// <summary>
    /// Ephemeral worker that stores the reply target once on start so continuation
    /// messages carry only domain data.
    /// </summary>
    public abstract class ReplyTargetWorkerActor : ReceiveActor
    {
        protected IActorRef ReplyTo { get; private set; } = ActorRefs.Nobody;

        protected void Begin(IActorRef replyTo)
        {
            ReplyTo = replyTo;
        }

        protected void Complete(object result)
        {
            ReplyTo.Tell(result);
            Context.Stop(Self);
        }

        protected void Fail(Exception exception)
        {
            ReplyTo.Tell(new Status.Failure(exception));
            Context.Stop(Self);
        }

        protected void Fail(object failureMessage)
        {
            ReplyTo.Tell(failureMessage);
            Context.Stop(Self);
        }
    }
}
