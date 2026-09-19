using Akka.Actor;
using Akka.Event;
using Contracts.Model.Enums;
using Domain.Model.Sync;
using Floor2Plan.Connectors.P6.Messages;
using Floor2Plan.Connectors.P6;
using Infrastructure.Process.Contracts.Scope;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Floor2Plan.Connectors.P6.Actors
{
    /// <summary>
    /// Subscribes to <see cref="IP6SyncProgressEvent"/> on the EventStream and writes each event to
    /// <see cref="IProcessLogger"/> using a fresh DI scope per message (safe for async background sync).
    /// </summary>
    public sealed class P6SyncProgressActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IServiceScopeFactory _scopeFactory;

        public P6SyncProgressActor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            Receive<IP6SyncProgressEvent>(HandleProgress);
            Context.System.EventStream.Subscribe(Self, typeof(IP6SyncProgressEvent));
        }

        internal sealed record SyncCompletedLogged;

        public static Props Props(IServiceScopeFactory scopeFactory)
        {
            return Akka.Actor.Props.Create(() => new P6SyncProgressActor(scopeFactory));
        }

        protected override void PostStop()
        {
            Context.System.EventStream.Unsubscribe(Self, typeof(IP6SyncProgressEvent));
            base.PostStop();
        }

        private void HandleProgress(IP6SyncProgressEvent progressEvent)
        {
            switch (progressEvent)
            {
                case P6SyncStarted started:
                    _log.Debug("P6 sync progress: started for {0} project(s)", started.ProjectCount);
                    WithProcessLogger(logger => logger.Log(new SyncLogMessageDto(
                        $"P6 sync started for {started.ProjectCount} project(s).",
                        SyncInformation.Information,
                        SyncType.Unknown)));
                    break;

                case P6ProjectStarted projectStarted:
                    WithProcessLogger(logger => logger.Log(new SyncLogMessageDto(
                        $"P6 sync starting for project ObjectId {projectStarted.ProjectObjectId} ({projectStarted.RemainingAfterThis} project(s) remaining after this one).",
                        SyncInformation.Information,
                        SyncType.Unknown)));
                    break;

                case P6ProjectCatalogFetched fetched:
                    WithProcessLogger(logger => logger.Log(new SyncLogMessageDto(
                        $"P6 sync fetched {fetched.Kind} for project ObjectId {fetched.ProjectObjectId}: {fetched.Count} records.",
                        SyncInformation.Total,
                        ToSyncType(fetched.Kind),
                        fetched.Count)));
                    break;

                case P6ProjectCatalogFetchFailed failed:
                    WithProcessLogger(logger => logger.Log(new SyncLogMessageDto(
                        $"P6 sync failed while fetching {failed.Kind} for project ObjectId {failed.ProjectObjectId}: {failed.Error}",
                        SyncInformation.Fail,
                        ToSyncType(failed.Kind))));
                    break;

                case P6SyncCompleted completed:
                    LogCompleted(completed.Result);
                    Context.Parent.Tell(new SyncCompletedLogged());
                    break;
            }
        }

        private void LogCompleted(P6SyncResult result)
        {
            WithProcessLogger(logger =>
            {
                logger.LogRange(new[]
                {
                    new SyncLogMessageDto("Projects retrieved from P6", SyncInformation.Total, SyncType.Project, result.ProjectCount),
                    new SyncLogMessageDto("WBS elements retrieved from P6", SyncInformation.Total, SyncType.Component, result.WbsCount),
                    new SyncLogMessageDto("Activities retrieved from P6", SyncInformation.Total, SyncType.Activity, result.ActivityCount),
                    new SyncLogMessageDto("Resources retrieved from P6", SyncInformation.Total, SyncType.Discipline, result.ResourceCount),
                    new SyncLogMessageDto("Resource assignments retrieved from P6", SyncInformation.Total, SyncType.Assignment, result.ResourceAssignmentCount),
                    new SyncLogMessageDto("Relationships retrieved from P6", SyncInformation.Total, SyncType.ActivityRelation, result.RelationshipCount)
                });

                logger.Log(new SyncLogMessageDto(
                    $"P6 sync retrieved {result.TotalRecordCount} records in total.",
                    result.Succeeded ? SyncInformation.Success : SyncInformation.Fail,
                    SyncType.Unknown));

                foreach (var error in result.Errors)
                {
                    logger.Log(new SyncLogMessageDto(error, SyncInformation.Fail, SyncType.Unknown));
                }
            });
        }

        private void WithProcessLogger(Action<IProcessLogger> write)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<IProcessLogger<P6Connector>>();
            write(logger);
        }

        private static SyncType ToSyncType(Api.Models.P6EntityKind kind)
        {
            return kind switch
            {
                Api.Models.P6EntityKind.Projects => SyncType.Project,
                Api.Models.P6EntityKind.Wbs => SyncType.Component,
                Api.Models.P6EntityKind.Activities => SyncType.Activity,
                Api.Models.P6EntityKind.Resources => SyncType.Discipline,
                Api.Models.P6EntityKind.ResourceAssignments => SyncType.Assignment,
                Api.Models.P6EntityKind.Relationships => SyncType.ActivityRelation,
                _ => SyncType.Unknown
            };
        }
    }
}
