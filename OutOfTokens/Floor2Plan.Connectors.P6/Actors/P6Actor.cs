using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Floor2Plan.Connectors.P6.Actors
{
    public sealed class P6Actor : ReceiveActor
    {
        public const string ActorName = "p6-client";

        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly IP6RestApi _api;
        private ICancelable _logoutSchedule;
        
        private readonly P6AuthOptions _authOptions;
        private readonly P6SyncOptions _syncOptions;
        private IActorRef _store;
        private IActorRef _originalSender;
        
        private string _sessionId;
        private readonly Dictionary<P6EntityKind, int> _catalogCounts = new ();
        private readonly Queue<P6EntityKind> _pendingCatalogs = new ();
        private readonly Queue<string> _pendingProjectIds = new ();
        private readonly List<string> _syncErrors = new ();
        private int _activeWorkers;
        private string _currentProjectId;
        private int? _currentProjectObjectId;
        
        public P6Actor(IP6RestApi api, P6AuthOptions authOptions, P6SyncOptions syncOptions = null)
        {
            _api = api;
            _authOptions = authOptions;
            _syncOptions = syncOptions ?? new P6SyncOptions();

            Receive<GetP6RawData>(message =>
            {
                _log.Info("Retrieving raw P6 data");
                message.Paging.Projects = [];
                message.Paging.Offset = 0;
                message.Paging.Kind = P6WorkflowKind.RawData;
                message.OriginalSender = Sender;

                var continueActor = Context.ActorOf(NewSessionOrContinueActor.Props(api, authOptions));
                continueActor.Tell(message);
            });

            Receive<GetP6Projects>(message =>
            {
                _log.Info("Retrieving P6 project catalog");
                
                message.Paging.Projects = [];
                message.Paging.Offset = 0;
                message.Paging.Kind = P6WorkflowKind.ProjectList;
                message.OriginalSender = Sender;

                var continueActor = Context.ActorOf(NewSessionOrContinueActor.Props(api, authOptions));
                continueActor.Tell(message);
            });

            Receive<NewSessionOrContinueActor.Continue>(msg =>
            {
                CancelIdleLogout();
                _sessionId = msg.SessionId;
                if (msg.Payload.Paging.Kind is P6WorkflowKind.ProjectList or P6WorkflowKind.RawData)
                {
                    _ = _api.GetProjectsAsync(
                            CreateSessionCookie(),
                            limit: IP6RestApi.ProjectSummaryPageSize,
                            offset: msg.Payload.Paging.Offset,
                            cancellationToken: CancellationToken.None)
                        .PipeTo(
                            Self,
                            msg.originalSender,
                            page => new ProjectSummaryPageReceived(msg.Payload.Paging.Projects.ToList(), msg.Payload.Paging.Offset, page, P6WorkflowKind.ProjectList),
                            exception => new ProjectSummaryPageFailed(exception));

                }
            });

            Receive<StartP6Sync>(message =>
            {
                if (_originalSender != null)
                {
                    Sender.Tell(new Status.Failure(new InvalidOperationException("A P6 synchronization is already running.")));
                    return;
                }

                var projectIds = (message.ProjectIds ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray();
                if (projectIds.Length == 0)
                {
                    Sender.Tell(new P6SyncResult(0, 0, 0, 0, 0, 0, new[] { "No projects selected to sync." }));
                    return;
                }

                _log.Info("P6 sync started for {0} project(s) with concurrency limit {1}", projectIds.Length, _syncOptions.MaxConcurrency);
                _originalSender = Sender;
                _pendingProjectIds.Clear();
                foreach (var projectId in projectIds)
                {
                    _pendingProjectIds.Enqueue(projectId);
                }

                _catalogCounts.Clear();
                _syncErrors.Clear();
                CancelIdleLogout();
                ContinueWithSyncSession();
            });

           
            Receive<SessionFailed>(message =>
            {
                _log.Warning("Could not login to P6: {0}", message.Reason);
                if (_originalSender != null)
                {
                    CompleteSync(new P6SyncResult(0, 0, 0, 0, 0, 0, new[] { message.Reason }));
                }
                else
                {
                    _originalSender.Tell(new Status.Failure(new InvalidOperationException(message.Reason)));
                }
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

            Receive<ProjectSummaryPageReceived>(message =>
            {
                if (message.Page == null)
                {
                    Sender.Tell(new Status.Failure(new InvalidOperationException("P6 project summary response was null.")));
                    LogoutCurrentSession();
                    return;
                }

                var list = message.Projects.ToList();
                list.AddRange(message.Page);
                if (message.Page.Count < IP6RestApi.ProjectSummaryPageSize)
                {
                    if (message.Workflow == P6WorkflowKind.ProjectList)
                    {
                        Sender.Tell(new P6Projects(list));
                        ScheduleIdleLogout();
                        return;
                    }

                    FetchActivityCounts(list);
                    return;
                }

                FetchProjectSummaryPage(
                    message.Projects,
                    message.Offset + IP6RestApi.ProjectSummaryPageSize,
                    message.Workflow);
            });

            Receive<ActivityCountsReceived>(message =>
            {
                Sender.Tell(CreateRawData(message.Projects, message.CountsByObjectId));
                ScheduleIdleLogout();
            });

            Receive<ActivityCountsFailed>(message =>
            {
                _log.Error(message.Exception, "Failed to compute per-project activity counts for raw P6 data");
                Sender.Tell(new Status.Failure(message.Exception));
                ScheduleIdleLogout();
            });

            Receive<ProjectSummaryPageFailed>(message =>
            {
                _log.Error(message.Exception, "Failed to retrieve raw P6 data");
                Sender.Tell(new Status.Failure(message.Exception));
                ScheduleIdleLogout();
            });

            Receive<SessionIdleTimeoutElapsed>(message =>
            {
                if (string.Equals(message.SessionId, _sessionId, StringComparison.Ordinal))
                {
                    LogoutCurrentSession();
                }
            });

            Receive<LogoutFailed>(message =>
            {
                _log.Error(message.Exception, "Failed to logout from P6");
            });

            Receive<LogoutCompleted>(message =>
            {
                using var response = message.Response;
                if (!response.IsSuccessStatusCode)
                {
                    _log.Warning("P6 logout failed with HTTP {0} {1}", (int)response.StatusCode, response.ReasonPhrase);
                }
            });
        }

        public static Props Props(IP6RestApi api, P6AuthOptions authOptions, P6SyncOptions syncOptions = null)
        {
            return Akka.Actor.Props.Create(() => new P6Actor(api, authOptions, syncOptions));
        }

        protected override void PreStart()
        {
            _store = Context.ActorOf(P6RawDataStoreActor.Props(), P6RawDataStoreActor.ActorName);
            base.PreStart();
        }

        protected override void PostStop()
        {
            CancelIdleLogout();
            LogoutCurrentSession();
            base.PostStop();
        }

        internal sealed record SessionReceived(string SessionId);

        internal sealed record SessionFailed(string Reason);

        private sealed record ProjectSummaryPageReceived(
            IList<P6ProjectRecord> Projects,
            int Offset,
            List<P6ProjectRecord> Page,
            P6WorkflowKind Workflow);

        private sealed record ProjectSummaryPageFailed(Exception Exception);

        private sealed record ActivityCountsReceived(
            IEnumerable<P6ProjectRecord> Projects,
            IReadOnlyDictionary<int, int> CountsByObjectId);

        private sealed record ActivityCountsFailed(Exception Exception);

        private sealed record LogoutFailed(Exception Exception);

        private sealed record LogoutCompleted(HttpResponseMessage Response);

        private sealed record SessionIdleTimeoutElapsed(string SessionId);

       
        private void ContinueWithSyncSession()
        {
            if (!string.IsNullOrWhiteSpace(_sessionId))
            {
                StartCatalogWorkers();
                return;
            }

            var loginActor = Context.ActorOf(P6LoginActor.Props(_api, _authOptions));
            loginActor.Tell(new P6LoginActor.LoginRequested());
        }

        private void StartCatalogWorkers()
        {
            _syncErrors.Clear();
            _catalogCounts.Clear();
            _store.Tell(new P6RawDataStoreActor.ClearRawData());
            _activeWorkers = 0;
            _currentProjectId = null;
            StartNextProject();
        }

        private void StartNextProject()
        {
            if (_pendingProjectIds.Count == 0)
            {
                var result = new P6SyncResult(
                    GetCatalogCount(P6EntityKind.Projects),
                    GetCatalogCount(P6EntityKind.Wbs),
                    GetCatalogCount(P6EntityKind.Activities),
                    GetCatalogCount(P6EntityKind.Resources),
                    GetCatalogCount(P6EntityKind.ResourceAssignments),
                    GetCatalogCount(P6EntityKind.Relationships),
                    _syncErrors.ToArray());
                CompleteSync(result);
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
            _log.Info("P6 sync starting for project ObjectId {0} ({1} project(s) remaining after this one)", _currentProjectId, _pendingProjectIds.Count);

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
                var worker = Context.ActorOf(P6CatalogWorkerActor.Props(_api, _store, Math.Max(1, _syncOptions.PageSize), projectFilter));
                _activeWorkers++;
                worker.Tell(new P6CatalogWorkerActor.FetchCatalog(kind, CreateSessionCookie(), 0));
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
            _log.Info(
                "P6 sync completed: {0} records, {1} errors",
                result.TotalRecordCount,
                result.Errors.Count);
            ScheduleIdleLogout();
        }

        private void FetchProjectSummaryPage(
            IEnumerable<P6ProjectRecord> projects,
            int offset,
            P6WorkflowKind workflow)
        {
            _ = _api.GetProjectsAsync(
                    CreateSessionCookie(),
                    limit: IP6RestApi.ProjectSummaryPageSize,
                    offset: offset,
                    cancellationToken: CancellationToken.None)
                .PipeTo(
                    Self,
                    Sender,
                    page => new ProjectSummaryPageReceived(projects.ToList(), offset, page, workflow),
                    exception => new ProjectSummaryPageFailed(exception));
        }

        /// <summary>
        /// Fetches the real activity count per project directly from the /activity endpoint, one call per
        /// project. This is only used for the raw-data connectivity check, where P6's cached
        /// SummaryActivityCount can be null/stale because it depends on P6 having run "Summarize Project".
        /// The live server ignores Limit and returns the full matching set in one response, so no paging
        /// is needed here - just one lightweight call (minimal fields) per selected project.
        /// </summary>
        private void FetchActivityCounts(List<P6ProjectRecord> projects)
        {
            var cookie = CreateSessionCookie();
            _ = FetchActivityCountsAsync(cookie, projects)
                .PipeTo(
                    Self,
                    Self,
                    counts => new ActivityCountsReceived(projects, counts),
                    exception => new ActivityCountsFailed(exception));
        }

        private async Task<IReadOnlyDictionary<int, int>> FetchActivityCountsAsync(string cookie, List<P6ProjectRecord> projects)
        {
            var counts = new Dictionary<int, int>();
            foreach (var project in projects)
            {
                var activities = await _api.GetActivitiesAsync(
                    cookie,
                    fields: "ObjectId,ProjectObjectId",
                    filter: $"ProjectObjectId={project.ObjectId}",
                    cancellationToken: CancellationToken.None);
                counts[project.ObjectId] = activities?.Count ?? 0;
            }

            return counts;
        }

        private void LogoutCurrentSession()
        {
            if (string.IsNullOrWhiteSpace(_sessionId))
            {
                return;
            }

            CancelIdleLogout();
            var cookie = CreateSessionCookie();
            _sessionId = null;
            _ = _api.LogoutAsync(cookie, CreateEmptyContent(), CancellationToken.None)
                .PipeTo(
                    Self,
                    Self,
                    response => new LogoutCompleted(response),
                    exception => new LogoutFailed(exception));
        }

        private void ScheduleIdleLogout()
        {
            if (string.IsNullOrWhiteSpace(_sessionId))
            {
                return;
            }

            CancelIdleLogout();
            _logoutSchedule = Context.System.Scheduler.ScheduleTellOnceCancelable(
                _authOptions.SessionIdleTimeout,
                Self,
                new SessionIdleTimeoutElapsed(_sessionId),
                Self);
        }

        private void CancelIdleLogout()
        {
            _logoutSchedule?.Cancel();
            _logoutSchedule = null;
        }

        private string CreateSessionCookie()
        {
            return $"{IP6RestApi.SessionCookieName}={_sessionId}";
        }

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new () { WriteIndented = true };
        private static P6RawData CreateRawData(IEnumerable<P6ProjectRecord> projects, IReadOnlyDictionary<int, int> activityCountsByObjectId)
        {
            foreach (var project in projects)
            {
                if (activityCountsByObjectId.TryGetValue(project.ObjectId, out var count))
                {
                    project.ComputedActivityCount = count;
                }
            }

            var payload = new P6ApiRawDataSnapshot
            {
                Projects = projects.ToList()
            };
            var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonSerializerOptions);

            return new P6RawData(
                new MemoryStream(bytes),
                "p6-raw-data.json",
                "application/json");
        }

        private static HttpContent CreateEmptyContent()
        {
            return new ByteArrayContent([]);
        }
    }
}
