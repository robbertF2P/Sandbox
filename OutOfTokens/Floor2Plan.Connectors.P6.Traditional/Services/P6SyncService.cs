using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Messages;
using Floor2Plan.Connectors.P6.Sync;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Floor2Plan.Connectors.P6.Traditional.Services
{
    /// <summary>
    /// Traditional async/await orchestration for a P6 sync run.
    /// </summary>
    public sealed class P6SyncService
    {
        private readonly P6SessionService _sessionService;
        private readonly P6PagedCatalogReader _catalogReader;
        private readonly P6SyncProgressReporter _progressReporter;
        private readonly P6RawDataStore _store;
        private readonly P6SyncOptions _syncOptions;
        private readonly ILogger<P6SyncService> _logger;
        private readonly SemaphoreSlim _syncGate = new(1, 1);

        public P6SyncService(
            P6SessionService sessionService,
            P6PagedCatalogReader catalogReader,
            P6SyncProgressReporter progressReporter,
            P6RawDataStore store,
            IOptions<P6SyncOptions> syncOptions,
            ILogger<P6SyncService> logger)
        {
            _sessionService = sessionService;
            _catalogReader = catalogReader;
            _progressReporter = progressReporter;
            _store = store;
            _syncOptions = syncOptions.Value;
            _logger = logger;
        }

        public bool TryQueueSync(
            IReadOnlyList<string> projectIds,
            IReadOnlyList<P6ProjectSyncPlan> syncPlans,
            out string rejectionReason)
        {
            if (!_syncGate.Wait(0))
            {
                rejectionReason = "A P6 synchronization is already running.";
                return false;
            }

            rejectionReason = null;
            _ = RunQueuedSyncAsync(projectIds, syncPlans);
            return true;
        }

        public async Task<P6SyncResult> RunSyncAsync(
            IReadOnlyList<string> projectIds,
            IReadOnlyList<P6ProjectSyncPlan> syncPlans = null,
            CancellationToken cancellationToken = default)
        {
            await _syncGate.WaitAsync(cancellationToken);
            try
            {
                return await ExecuteSyncAsync(projectIds, syncPlans, cancellationToken);
            }
            finally
            {
                _syncGate.Release();
            }
        }

        private async Task RunQueuedSyncAsync(
            IReadOnlyList<string> projectIds,
            IReadOnlyList<P6ProjectSyncPlan> syncPlans)
        {
            try
            {
                await ExecuteSyncAsync(projectIds, syncPlans, CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Queued P6 sync failed.");
                _progressReporter.Report(new P6SyncCompleted(new P6SyncResult(0, 0, 0, 0, 0, 0, [exception.Message])));
            }
            finally
            {
                _syncGate.Release();
            }
        }

        private async Task<P6SyncResult> ExecuteSyncAsync(
            IReadOnlyList<string> projectIds,
            IReadOnlyList<P6ProjectSyncPlan> syncPlans,
            CancellationToken cancellationToken)
        {
            var plans = ResolvePlans(projectIds, syncPlans);
            var errors = new List<string>();
            var catalogCounts = new Dictionary<P6EntityKind, int>();

            _store.Clear();
            _progressReporter.Report(new P6SyncStarted(plans.Count));
            _logger.LogInformation(
                "P6 sync started for {ProjectCount} project(s) with concurrency limit {MaxConcurrency}",
                plans.Count,
                _syncOptions.MaxConcurrency);

            await using var session = await _sessionService.OpenSessionAsync(cancellationToken);
            var remainingAfterCurrent = plans.Count;

            foreach (var plan in plans)
            {
                remainingAfterCurrent--;
                if (!int.TryParse(plan.ProjectObjectId, out var objectId))
                {
                    errors.Add(
                        $"Project {plan.ProjectObjectId}: stored value is not a valid P6 ObjectId (project selection may need to be re-saved).");
                    continue;
                }

                _progressReporter.Report(new P6ProjectStarted(objectId, remainingAfterCurrent));
                _logger.LogInformation(
                    "P6 sync starting for project ObjectId {ProjectObjectId}",
                    plan.ProjectObjectId);

                await FetchProjectCatalogsAsync(
                    plan,
                    objectId,
                    session.Cookie,
                    catalogCounts,
                    errors,
                    cancellationToken);
            }

            var result = new P6SyncResult(
                GetCount(catalogCounts, P6EntityKind.Projects),
                GetCount(catalogCounts, P6EntityKind.Wbs),
                GetCount(catalogCounts, P6EntityKind.Activities),
                GetCount(catalogCounts, P6EntityKind.Resources),
                GetCount(catalogCounts, P6EntityKind.ResourceAssignments),
                GetCount(catalogCounts, P6EntityKind.Relationships),
                errors);

            _progressReporter.Report(new P6SyncCompleted(result));
            _logger.LogInformation(
                "P6 sync completed: {RecordCount} records, {ErrorCount} errors",
                result.TotalRecordCount,
                result.Errors.Count);

            return result;
        }

        private async Task FetchProjectCatalogsAsync(
            P6ProjectSyncPlan plan,
            int objectId,
            string cookie,
            Dictionary<P6EntityKind, int> catalogCounts,
            List<string> errors,
            CancellationToken cancellationToken)
        {
            var pageSize = Math.Max(1, _syncOptions.PageSize);
            var maxConcurrency = Math.Max(1, _syncOptions.MaxConcurrency);
            using var throttle = new SemaphoreSlim(maxConcurrency, maxConcurrency);

            var workItems = plan.GetEntityKinds()
                .Select(kind => new CatalogWorkItem(plan, objectId, kind, plan.BuildFilter(kind, objectId)))
                .ToList();

            var tasks = workItems.Select(async item =>
            {
                await throttle.WaitAsync(cancellationToken);
                try
                {
                    await FetchCatalogAsync(item, cookie, pageSize, catalogCounts, errors, cancellationToken);
                }
                finally
                {
                    throttle.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        private async Task FetchCatalogAsync(
            CatalogWorkItem item,
            string cookie,
            int pageSize,
            Dictionary<P6EntityKind, int> catalogCounts,
            List<string> errors,
            CancellationToken cancellationToken)
        {
            try
            {
                var count = await _catalogReader.FetchAllPagesAsync(
                    item.Kind,
                    cookie,
                    item.ProjectFilter,
                    pageSize,
                    page => _store.Append(item.Kind, page),
                    cancellationToken);

                lock (catalogCounts)
                {
                    catalogCounts[item.Kind] = catalogCounts.GetValueOrDefault(item.Kind) + count;
                }

                _logger.LogInformation(
                    "P6 sync fetched {Kind} for project {ProjectObjectId}: {Count} records",
                    item.Kind,
                    item.Plan.ProjectObjectId,
                    count);
                _progressReporter.Report(new P6ProjectCatalogFetched(item.ProjectObjectId, item.Kind, count));
            }
            catch (Exception exception)
            {
                var error = $"{item.Kind}: {exception.Message}";
                lock (errors)
                {
                    errors.Add(error);
                }

                _logger.LogError(exception, "P6 sync failed while fetching {Kind}", item.Kind);
                _progressReporter.Report(new P6ProjectCatalogFetchFailed(
                    item.ProjectObjectId,
                    item.Kind,
                    exception.Message));
            }
        }

        private static IReadOnlyList<P6ProjectSyncPlan> ResolvePlans(
            IReadOnlyList<string> projectIds,
            IReadOnlyList<P6ProjectSyncPlan> syncPlans)
        {
            if (syncPlans is { Count: > 0 })
            {
                return syncPlans;
            }

            return projectIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Select(P6ProjectSyncPlan.Full)
                .ToList();
        }

        private static int GetCount(Dictionary<P6EntityKind, int> catalogCounts, P6EntityKind kind)
        {
            return catalogCounts.GetValueOrDefault(kind);
        }

        private sealed record CatalogWorkItem(
            P6ProjectSyncPlan Plan,
            int ProjectObjectId,
            P6EntityKind Kind,
            string ProjectFilter);
    }
}
