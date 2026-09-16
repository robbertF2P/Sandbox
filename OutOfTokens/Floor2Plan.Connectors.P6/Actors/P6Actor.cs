using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Messages;
using System;
using System.Linq;

namespace Floor2Plan.Connectors.P6.Actors
{
    /// <summary>
    /// Front door for the P6 connector actor pipeline. Routes requests to the session actor and owns the raw-data store.
    /// </summary>
    public sealed class P6Actor : ReceiveActor
    {
        public const string ActorName = "p6-client";

        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IP6RestApi _api;
        private readonly P6AuthOptions _authOptions;
        private readonly P6SyncOptions _syncOptions;

        private IActorRef _session = ActorRefs.Nobody;
        private IActorRef _store = ActorRefs.Nobody;
        private bool _syncRunning;

        public P6Actor(IP6RestApi api, P6AuthOptions authOptions, P6SyncOptions? syncOptions = null)
        {
            _api = api;
            _authOptions = authOptions;
            _syncOptions = syncOptions ?? new P6SyncOptions();

            Receive<GetP6RawData>(message =>
            {
                _ = message;
                _log.Info("Retrieving raw P6 data");
                _session.Tell(new BuildRawData(Sender));
            });

            Receive<GetP6Projects>(message =>
            {
                _ = message;
                _log.Info("Retrieving P6 project catalog");
                _session.Tell(new FetchProjectCatalog(Sender));
            });

            Receive<StartP6Sync>(message =>
            {
                if (_syncRunning)
                {
                    Sender.Tell(new Status.Failure(new InvalidOperationException("A P6 synchronization is already running.")));
                    return;
                }

                var projectIds = (message.ProjectIds ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray();
                if (projectIds.Length == 0)
                {
                    Sender.Tell(new P6SyncResult(0, 0, 0, 0, 0, 0, ["No projects selected to sync."]));
                    return;
                }

                _syncRunning = true;
                _session.Tell(new RunSync(Sender, projectIds, _store, _syncOptions));
            });

            Receive<P6SyncOrchestratorActor.SyncFinished>(_ =>
            {
                _syncRunning = false;
            });
        }

        public static Props Props(IP6RestApi api, P6AuthOptions authOptions, P6SyncOptions? syncOptions = null)
        {
            return Akka.Actor.Props.Create(() => new P6Actor(api, authOptions, syncOptions));
        }

        protected override void PreStart()
        {
            _store = Context.ActorOf(P6RawDataStoreActor.Props(), P6RawDataStoreActor.ActorName);
            _session = Context.ActorOf(P6SessionActor.Props(_api, _authOptions), P6SessionActor.ActorName);
            base.PreStart();
        }
    }
}
