using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Messages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Floor2Plan.Connectors.P6.Traditional.Services
{
    /// <summary>
    /// Builds the downloadable raw-data snapshot.
    /// </summary>
    public sealed class P6RawDataExporter
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        private readonly P6SessionService _sessionService;
        private readonly P6PagedCatalogReader _catalogReader;
        private readonly IP6RestApi _api;

        public P6RawDataExporter(
            P6SessionService sessionService,
            P6PagedCatalogReader catalogReader,
            IP6RestApi api)
        {
            _sessionService = sessionService;
            _catalogReader = catalogReader;
            _api = api;
        }

        public async Task<P6RawData> ExportAsync(CancellationToken cancellationToken = default)
        {
            await using var session = await _sessionService.OpenSessionAsync(cancellationToken);
            var projects = await _catalogReader.FetchProjectsAsync(session.Cookie, cancellationToken);
            var activityCounts = await FetchActivityCountsAsync(session.Cookie, projects, cancellationToken);

            foreach (var project in projects)
            {
                if (activityCounts.TryGetValue(project.ObjectId, out var count))
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

        private async Task<IReadOnlyDictionary<int, int>> FetchActivityCountsAsync(
            string cookie,
            IReadOnlyList<P6ProjectRecord> projects,
            CancellationToken cancellationToken)
        {
            var counts = new Dictionary<int, int>();
            foreach (var project in projects)
            {
                var activities = await _api.GetActivitiesAsync(
                    cookie,
                    fields: "ObjectId,ProjectObjectId",
                    filter: $"ProjectObjectId={project.ObjectId}",
                    cancellationToken: cancellationToken);
                counts[project.ObjectId] = activities?.Count ?? 0;
            }

            return counts;
        }
    }
}
