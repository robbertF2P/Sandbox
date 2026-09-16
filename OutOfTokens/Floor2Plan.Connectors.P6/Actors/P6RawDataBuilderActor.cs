using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Floor2Plan.Connectors.P6.Actors
{
    /// <summary>
    /// Builds the downloadable raw-data snapshot: project catalog plus live activity counts per project.
    /// </summary>
    public sealed class P6RawDataBuilderActor : ReceiveActor
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IP6RestApi _api;

        private string _cookie = string.Empty;
        private IActorRef _replyTo = ActorRefs.Nobody;

        public P6RawDataBuilderActor(IP6RestApi api)
        {
            _api = api;
            Receive<Build>(StartBuild);
        }

        public static Props Props(IP6RestApi api)
        {
            return Akka.Actor.Props.Create(() => new P6RawDataBuilderActor(api));
        }

        internal sealed record Build(string Cookie, IActorRef ReplyTo);

        private sealed record ActivityCountsReady(
            List<P6ProjectRecord> Projects,
            IReadOnlyDictionary<int, int> CountsByObjectId);

        private sealed record ActivityCountsFailed(Exception Exception);

        private void StartBuild(Build message)
        {
            _cookie = message.Cookie;
            _replyTo = message.ReplyTo;
            _log.Info("Building P6 raw data export");

            var catalog = Context.ActorOf(P6ProjectCatalogActor.Props(_api));
            catalog.Tell(new P6ProjectCatalogActor.Fetch(_cookie, Self));
            Become(WaitingForProjects);
        }

        private void WaitingForProjects()
        {
            Receive<P6Projects>(message =>
            {
                _ = FetchActivityCountsAsync(_cookie, message.Projects.ToList())
                    .PipeTo(
                        Self,
                        Self,
                        counts => new ActivityCountsReady(message.Projects.ToList(), counts),
                        exception => new ActivityCountsFailed(exception));
                Become(WaitingForActivityCounts);
            });

            Receive<Status.Failure>(message =>
            {
                _replyTo.Tell(message);
                Context.Stop(Self);
            });
        }

        private void WaitingForActivityCounts()
        {
            Receive<ActivityCountsReady>(message =>
            {
                _replyTo.Tell(CreateRawData(message.Projects, message.CountsByObjectId));
                Context.Stop(Self);
            });

            Receive<ActivityCountsFailed>(message =>
            {
                _log.Error(message.Exception, "Failed to compute per-project activity counts for raw P6 data");
                _replyTo.Tell(new Status.Failure(message.Exception));
                Context.Stop(Self);
            });
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
            var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);

            return new P6RawData(
                new MemoryStream(bytes),
                "p6-raw-data.json",
                "application/json");
        }
    }
}
