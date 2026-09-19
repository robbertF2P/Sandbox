using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Messages;
using Floor2Plan.Connectors.P6.Sync;
using Infrastructure.Akka.Actors.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Floor2Plan.Connectors.P6.Actors
{
    /// <summary>
    /// Orchestrates a P6 synchronization run for the selected project plans.
    /// Each project is processed sequentially; catalogs within a project run in parallel via <see cref="BatchOrchestratorActor{TWorkItem}"/>.
    /// </summary>
    public sealed class P6SyncOrchestratorActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IP6RestApi _api;
        private readonly IActorRef _store;
        private readonly P6SyncOptions _syncOptions;

        private readonly Dictionary<P6EntityKind, int> _catalogCounts = new();
        private readonly Queue<P6ProjectSyncPlan> _pendingPlans = new();
        private readonly List<string> _syncErrors = new();

        private string _sessionCookie = string.Empty;
        private IActorRef _replyTo = ActorRefs.Nobody;

        public P6SyncOrchestratorActor(IP6RestApi api, IActorRef store, P6SyncOptions syncOptions)
        {
            _api = api;
            _store = store;
            _syncOptions = syncOptions;

            Receive<Start>(message =>
            {
                _sessionCookie = message.Cookie;
                _replyTo = message.ReplyTo;
                _pendingPlans.Clear();
                foreach (var plan in ResolvePlans(message))
                {
                    _pendingPlans.Enqueue(plan);
                }

                _log.Info(
                    "P6 sync started for {0} project(s) with concurrency limit {1}",
                    _pendingPlans.Count,
                    _syncOptions.MaxConcurrency);

                _catalogCounts.Clear();
                _syncErrors.Clear();
                _store.Tell(new P6RawDataStoreActor.ClearRawData());
                StartNextProjectBatch();
            });

            Receive<ProjectBatchComplete>(_ => StartNextProjectBatch());
        }

        public static Props Props(IP6RestApi api, IActorRef store, P6SyncOptions syncOptions)
        {
            return Akka.Actor.Props.Create(() => new P6SyncOrchestratorActor(api, store, syncOptions));
        }

        internal sealed record Start(
            IReadOnlyList<string> ProjectIds,
            string Cookie,
            IActorRef ReplyTo,
            IReadOnlyList<P6ProjectSyncPlan> SyncPlans = null);

        internal sealed record SyncFinished;

        private sealed record ProjectBatchComplete;

        private void StartNextProjectBatch()
        {
            if (_pendingPlans.Count == 0)
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

            var plan = _pendingPlans.Dequeue();
            if (!int.TryParse(plan.ProjectObjectId, out var objectId))
            {
                _syncErrors.Add(
                    $"Project {plan.ProjectObjectId}: stored value is not a valid P6 ObjectId (project selection may need to be re-saved).");
                StartNextProjectBatch();
                return;
            }

            _log.Info(
                "P6 sync starting for project ObjectId {0} ({1} project(s) remaining after this one)",
                plan.ProjectObjectId,
                _pendingPlans.Count);

            var workItems = plan.GetEntityKinds()
                .Select(kind => new P6CatalogSyncWorkItem(
                    plan,
                    objectId,
                    kind,
                    _sessionCookie,
                    plan.BuildFilter(kind, objectId)))
                .ToList();

            var batch = Context.ActorOf(
                BatchOrchestratorActor<P6CatalogSyncWorkItem>.Props(CreateProjectBatchOptions()),
                $"p6-project-batch-{plan.ProjectObjectId}");
            batch.Tell(new BatchOrchestratorActor<P6CatalogSyncWorkItem>.Start(workItems, Self));
        }

        private BatchOrchestratorOptions<P6CatalogSyncWorkItem> CreateProjectBatchOptions()
        {
            return new BatchOrchestratorOptions<P6CatalogSyncWorkItem>
            {
                MaxConcurrency = _syncOptions.MaxConcurrency,
                CreateWorker = item => (
                    P6CatalogWorkerActor.Props(
                        _api,
                        _store,
                        Math.Max(1, _syncOptions.PageSize),
                        item.ProjectFilter),
                    new P6CatalogWorkerActor.FetchCatalog(item.Kind, item.Cookie, 0)),
                IsSuccess = (item, message) =>
                    message is P6CatalogWorkerActor.CatalogFetched fetched && fetched.Kind == item.Kind,
                IsFailure = (item, message) =>
                    message is P6CatalogWorkerActor.CatalogFetchFailed failed && failed.Kind == item.Kind,
                OnSuccess = (item, message, context) =>
                {
                    var fetched = (P6CatalogWorkerActor.CatalogFetched)message;
                    _catalogCounts[item.Kind] = _catalogCounts.TryGetValue(item.Kind, out var existing)
                        ? existing + fetched.Count
                        : fetched.Count;
                    _log.Info(
                        "P6 sync fetched {0} for project {1}: {2} records",
                        item.Kind,
                        item.Plan.ProjectObjectId,
                        fetched.Count);
                },
                OnFailure = (item, message, context) =>
                {
                    var failed = (P6CatalogWorkerActor.CatalogFetchFailed)message;
                    var error = $"{failed.Kind}: {failed.Exception.Message}";
                    context.Errors.Add(error);
                    _syncErrors.Add(error);
                    _log.Error(failed.Exception, "P6 sync failed while fetching {0}", failed.Kind);
                },
                BuildReply = _ => new ProjectBatchComplete()
            };
        }

        private static IReadOnlyList<P6ProjectSyncPlan> ResolvePlans(Start message)
        {
            if (message.SyncPlans is { Count: > 0 })
            {
                return message.SyncPlans;
            }

            return message.ProjectIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Select(P6ProjectSyncPlan.Full)
                .ToList();
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
