using Akka.Actor;
using Akka.Event;
using Contracts.Model.Enums;
using Domain.Model.Sync;
using Floor2Plan.Connectors.P6.Messages;
using Floor2Plan.Connectors.P6;
using Infrastructure.Process.Contracts.Scope;
using Microsoft.Extensions.DependencyInjection;

namespace Floor2Plan.Connectors.P6.Actors
{
    /// <summary>
    /// Receives P6 sync progress events and writes each one to <see cref="IProcessLogger"/>
    /// using a fresh DI scope per message (safe for async background sync).
    /// </summary>
    public sealed class P6SyncProgressActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IServiceScopeFactory _scopeFactory;

        public P6SyncProgressActor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Receive<P6SyncStarted>(message =>
            {
                _log.Debug("P6 sync progress: started for {0} project(s)", message.ProjectCount);
                WithProcessLogger(logger => logger.Log(new SyncLogMessageDto(
                    $"P6 sync started for {message.ProjectCount} project(s).",
                    SyncInformation.Information,
                    SyncType.Unknown)));
            });

            Receive<P6ProjectStarted>(message =>
            {
                WithProcessLogger(logger => logger.Log(new SyncLogMessageDto(
                    $"P6 sync starting for project ObjectId {message.ProjectObjectId} ({message.RemainingAfterThis} project(s) remaining after this one).",
                    SyncInformation.Information,
                    SyncType.Unknown)));
            });

            Receive<P6ProjectCatalogFetched>(message =>
            {
                WithProcessLogger(logger => logger.Log(new SyncLogMessageDto(
                    $"P6 sync fetched {message.Kind} for project ObjectId {message.ProjectObjectId}: {message.Count} records.",
                    SyncInformation.Total,
                    ToSyncType(message.Kind),
                    message.Count)));
            });

            Receive<P6ProjectCatalogFetchFailed>(message =>
            {
                WithProcessLogger(logger => logger.Log(new SyncLogMessageDto(
                    $"P6 sync failed while fetching {message.Kind} for project ObjectId {message.ProjectObjectId}: {message.Error}",
                    SyncInformation.Fail,
                    ToSyncType(message.Kind))));
            });

            Receive<P6SyncCompleted>(message =>
            {
                var result = message.Result;
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

                Context.Parent.Tell(new SyncCompletedLogged());
            });
        }

        internal sealed record SyncCompletedLogged;

        public static Props Props(IServiceScopeFactory scopeFactory)
        {
            return Akka.Actor.Props.Create(() => new P6SyncProgressActor(scopeFactory));
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
