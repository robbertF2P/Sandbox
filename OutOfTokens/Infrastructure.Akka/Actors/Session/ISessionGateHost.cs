using Akka.Actor;
using Akka.Event;

namespace Infrastructure.Akka.Actors.Session
{
    public interface ISessionGateHost
    {
        IActorRef Self { get; }

        IActorRef Parent { get; }

        ILoggingAdapter Log { get; }

        ActorSystem ActorSystem { get; }

        object SessionContext { get; set; }

        bool IsLoggingIn { get; }

        void Receive<T>(Action<T> handler);

        void OnWork<T>(Action<T> whenReady);

        void Stash();

        void UnstashAll();

        void CompleteLogin(object sessionContext);

        void FailLogin(string reason);

        void WatchWorker(IActorRef worker);

        void ScheduleIdleLogout(object sessionToken);

        void CancelIdleLogout();

        void BeginLogin(object work);

        IActorRef ActorOf(Props props);
    }
}
