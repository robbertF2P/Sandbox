using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Floor2Plan.Connectors.P6.Actors
{
    /// <summary>
    /// Orchestrates a full P6 synchronization run for the selected project ObjectIds.
    /// </summary>
    public sealed class P6SyncOrchestratorActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IP6RestApi _api;
        private readonly IActorRef _store;
        private readonly P6SyncOptions _syncOptions;

        private readonly Dictionary<P6EntityKind, int> _catalogCounts = new();
        private readonly Queue<P6EntityKind> _pendingCatalogs = new();
        private readonly Queue<string> _pendingProjectIds = new();
        private readonly List<string> _syncErrors = new();

        private string _sessionCookie = string.Empty;
        private IActorRef _replyTo = ActorRefs.Nobody;
        private int _activeWorkers;
        private string? _currentProjectId;
        private int? _currentProjectObjectId;

        public P6SyncOrchestratorActor(IP6RestApi api, IActorRef store, P6SyncOptions syncOptions)
        {
            _api = api;
            _store = store;
            _syncOptions = syncOptions;

            Receive<Start>(message =>
            {
                _sessionCookie = message.Cookie;
                _replyTo = message.ReplyTo;
                _pendingProjectIds.Clear();
                foreach (var projectId in message.ProjectIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
                {
                    _pendingProjectIds.Enqueue(projectId);
                }

                _log.Info(
                    "P6 sync started for {0} project(s) with concurrency limit {1}",
                    _pendingProjectIds.Count,
                    _syncOptions.MaxConcurrency);

                _catalogCounts.Clear();
                _syncErrors.Clear();
                _store.Tell(new P6RawDataStoreActor.ClearRawData());
                _activeWorkers = 0;
                StartNextProject();
            });

            Receive<P6CatalogWorkerActor.CatalogFetched>(message =>
            {
                _activeWorkers--;
                _catalogCounts[message.Kind] = _catalogCounts.TryGetValue(message.Kind, out var existing)
                    ? existing + message.Count
                    : message.Count;
                _log.Info("P6 sync fetched {0} for project {1}: {2} records", message.Kind, _currentProjectId, message.Count);
                StartNextCatalogWorkers();
            });

            Receive<P6CatalogWorkerActor.CatalogFetchFailed>(message =>
            {
                _activeWorkers--;
                _syncErrors.Add($"{message.Kind}: {message.Exception.Message}");
                _log.Error(message.Exception, "P6 sync failed while fetching {0}", message.Kind);
                StartNextCatalogWorkers();
            });
        }

        public static Props Props(IP6RestApi api, IActorRef store, P6SyncOptions syncOptions)
        {
            return Akka.Actor.Props.Create(() => new P6SyncOrchestratorActor(api, store, syncOptions));
        }

        internal sealed record Start(IReadOnlyList<string> ProjectIds, string Cookie, IActorRef ReplyTo);

        internal sealed record SyncFinished;

        private void StartNextProject()
        {
            if (_pendingProjectIds.Count == 0)
            {
                CompleteSync(new P6SyncResult(
                    GetCatalogCount(P6EntityKind.Projects),
                    GetCatalogCount(P6EntityKind.Wbs),
                    GetCatalogCount(P6EntityKind.Activities),
                    GetCatalogCount(P6EntityKind.Resources),
                    GetCatalogCount(P6EntityKind.ResourceAssignments),
                    GetCatalogCount(P6EntityKind.Relationships),
                    _syncErrors.ToArray()));
                return;
            }

            _currentProjectId = _pendingProjectIds.Dequeue();
            if (!int.TryParse(_currentProjectId, out var objectId))
            {
                _syncErrors.Add($"Project {_currentProjectId}: stored value is not a valid P6 ObjectId (project selection may need to be re-saved).");
                StartNextProject();
                return;
            }

            _currentProjectObjectId = objectId;
            _log.Info(
                "P6 sync starting for project ObjectId {0} ({1} project(s) remaining after this one)",
                _currentProjectId,
                _pendingProjectIds.Count);

            _pendingCatalogs.Clear();
            foreach (var kind in Enum.GetValues<P6EntityKind>())
            {
                _pendingCatalogs.Enqueue(kind);
            }

            StartNextCatalogWorkers();
        }

        private void StartNextCatalogWorkers()
        {
            while (_activeWorkers < Math.Max(1, _syncOptions.MaxConcurrency) && _pendingCatalogs.Count > 0)
            {
                var kind = _pendingCatalogs.Dequeue();
                var projectFilter = kind switch
                {
                    P6EntityKind.Projects => $"ObjectId={_currentProjectObjectId}",
                    P6EntityKind.Relationships => $"PredecessorProjectObjectId={_currentProjectObjectId}",
                    _ => $"ProjectObjectId={_currentProjectObjectId}"
                };
                var worker = Context.ActorOf(
                    P6CatalogWorkerActor.Props(_api, _store, Math.Max(1, _syncOptions.PageSize), projectFilter));
                _activeWorkers++;
                worker.Tell(new P6CatalogWorkerActor.FetchCatalog(kind, _sessionCookie, 0));
            }

            if (_activeWorkers == 0 && _pendingCatalogs.Count == 0)
            {
                StartNextProject();
            }
        }

        private int GetCatalogCount(P6EntityKind kind)
        {
            return _catalogCounts.GetValueOrDefault(kind, 0);
        }

        private void CompleteSync(P6SyncResult result)
        {
            _log.Info("P6 sync completed: {0} records, {1} errors", result.TotalRecordCount, result.Errors.Count);
            _replyTo.Tell(result);
            Context.Parent.Tell(new SyncFinished());
            Context.Stop(Self);
        }
    }
}
