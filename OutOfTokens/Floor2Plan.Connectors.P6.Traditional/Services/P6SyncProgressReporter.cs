using Contracts.Model.Enums;
using Domain.Model.Sync;
using Floor2Plan.Connectors.P6;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Messages;
using Infrastructure.Process.Contracts.Scope;

namespace Floor2Plan.Connectors.P6.Traditional.Services
{
    /// <summary>
    /// Maps sync progress events to the Floor2Plan sync log.
    /// </summary>
    public sealed class P6SyncProgressReporter
    {
        private readonly IProcessLogger<P6TraditionalConnector> _processLogger;

        public P6SyncProgressReporter(IProcessLogger<P6TraditionalConnector> processLogger)
        {
            _processLogger = processLogger;
        }

        public void Report(IP6SyncProgressEvent progressEvent)
        {
            switch (progressEvent)
            {
                case P6SyncStarted started:
                    _processLogger.Log(new SyncLogMessageDto(
                        $"P6 sync started for {started.ProjectCount} project(s).",
                        SyncInformation.Information,
                        SyncType.Unknown));
                    break;

                case P6ProjectStarted projectStarted:
                    _processLogger.Log(new SyncLogMessageDto(
                        $"P6 sync starting for project ObjectId {projectStarted.ProjectObjectId} ({projectStarted.RemainingAfterThis} project(s) remaining after this one).",
                        SyncInformation.Information,
                        SyncType.Unknown));
                    break;

                case P6ProjectCatalogFetched fetched:
                    _processLogger.Log(new SyncLogMessageDto(
                        $"P6 sync fetched {fetched.Kind} for project ObjectId {fetched.ProjectObjectId}: {fetched.Count} records.",
                        SyncInformation.Total,
                        ToSyncType(fetched.Kind),
                        fetched.Count));
                    break;

                case P6ProjectCatalogFetchFailed failed:
                    _processLogger.Log(new SyncLogMessageDto(
                        $"P6 sync failed while fetching {failed.Kind} for project ObjectId {failed.ProjectObjectId}: {failed.Error}",
                        SyncInformation.Fail,
                        ToSyncType(failed.Kind)));
                    break;

                case P6SyncCompleted completed:
                    LogCompleted(completed.Result);
                    break;
            }
        }

        private void LogCompleted(P6SyncResult result)
        {
            _processLogger.LogRange(new[]
            {
                new SyncLogMessageDto("Projects retrieved from P6", SyncInformation.Total, SyncType.Project, result.ProjectCount),
                new SyncLogMessageDto("WBS elements retrieved from P6", SyncInformation.Total, SyncType.Component, result.WbsCount),
                new SyncLogMessageDto("Activities retrieved from P6", SyncInformation.Total, SyncType.Activity, result.ActivityCount),
                new SyncLogMessageDto("Resources retrieved from P6", SyncInformation.Total, SyncType.Discipline, result.ResourceCount),
                new SyncLogMessageDto("Resource assignments retrieved from P6", SyncInformation.Total, SyncType.Assignment, result.ResourceAssignmentCount),
                new SyncLogMessageDto("Relationships retrieved from P6", SyncInformation.Total, SyncType.ActivityRelation, result.RelationshipCount)
            });

            _processLogger.Log(new SyncLogMessageDto(
                $"P6 sync retrieved {result.TotalRecordCount} records in total.",
                result.Succeeded ? SyncInformation.Success : SyncInformation.Fail,
                SyncType.Unknown));

            foreach (var error in result.Errors)
            {
                _processLogger.Log(new SyncLogMessageDto(error, SyncInformation.Fail, SyncType.Unknown));
            }
        }

        private static SyncType ToSyncType(P6EntityKind kind)
        {
            return kind switch
            {
                P6EntityKind.Projects => SyncType.Project,
                P6EntityKind.Wbs => SyncType.Component,
                P6EntityKind.Activities => SyncType.Activity,
                P6EntityKind.Resources => SyncType.Discipline,
                P6EntityKind.ResourceAssignments => SyncType.Assignment,
                P6EntityKind.Relationships => SyncType.ActivityRelation,
                _ => SyncType.Unknown
            };
        }
    }
}
