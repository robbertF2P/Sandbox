using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Messages;
using Floor2Plan.Connectors.P6.Sync;
using Infrastructure.Akka.Actors.Guards;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
        private readonly IServiceScopeFactory _scopeFactory;

        private IActorRef _session = ActorRefs.Nobody;
        private IActorRef _store = ActorRefs.Nobody;
        private IActorRef _syncGate = ActorRefs.Nobody;

        public P6Actor(
            IP6RestApi api,
            P6AuthOptions authOptions,
            P6SyncOptions syncOptions = null,
            IServiceScopeFactory scopeFactory = null)
        {
            _api = api;
            _authOptions = authOptions;
            _syncOptions = syncOptions ?? new P6SyncOptions();
            _scopeFactory = scopeFactory;

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
                var (projectIds, _) = ResolveSyncRequest(message);
                if (projectIds.Length == 0)
                {
                    Sender.Tell(new P6SyncResult(0, 0, 0, 0, 0, 0, ["No projects selected to sync."]));
                    return;
                }

                _syncGate.Tell(new ExclusiveGateActor.Begin(message, Sender));
            });

            Receive<P6SyncOrchestratorActor.SyncFinished>(_ =>
            {
                _syncGate.Tell(new ExclusiveGateActor.Finished());
            });
        }

        public static Props Props(
            IP6RestApi api,
            P6AuthOptions authOptions,
            P6SyncOptions syncOptions = null,
            IServiceScopeFactory scopeFactory = null)
        {
            return Akka.Actor.Props.Create(() => new P6Actor(api, authOptions, syncOptions, scopeFactory));
        }

        protected override void PreStart()
        {
            _store = Context.ActorOf(P6RawDataStoreActor.Props(), P6RawDataStoreActor.ActorName);
            if (_scopeFactory != null)
            {
                Context.ActorOf(P6SyncProgressActor.Props(_scopeFactory), "p6-sync-progress");
            }

            _session = Context.ActorOf(P6SessionActor.Props(_api, _authOptions), P6SessionActor.ActorName);
            _syncGate = Context.ActorOf(
                ExclusiveGateActor.Props(new ExclusiveGateOptions
                {
                    OnBegin = (work, replyTo) =>
                    {
                        var message = (StartP6Sync)work;
                        var (projectIds, syncPlans) = ResolveSyncRequest(message);
                        _session.Tell(new RunSync(replyTo, projectIds, _store, _syncOptions, syncPlans));
                    },
                    BuildRejection = _ => new Status.Failure(
                        new InvalidOperationException("A P6 synchronization is already running."))
                }),
                "p6-sync-gate");
            base.PreStart();
        }

        private static (string[] ProjectIds, IReadOnlyList<P6ProjectSyncPlan> SyncPlans) ResolveSyncRequest(StartP6Sync message)
        {
            var syncPlans = message.SyncPlans?
                .Where(plan => !string.IsNullOrWhiteSpace(plan.ProjectObjectId))
                .ToArray();

            if (syncPlans is { Length: > 0 })
            {
                return (syncPlans
                    .Select(plan => plan.ProjectObjectId)
                    .Distinct()
                    .ToArray(), syncPlans);
            }

            var projectIds = (message.ProjectIds ?? [])
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToArray();

            return (projectIds, null);
        }
    }
}
