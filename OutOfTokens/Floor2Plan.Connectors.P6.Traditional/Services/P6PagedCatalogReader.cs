using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Floor2Plan.Connectors.P6.Traditional.Services
{
    /// <summary>
    /// Pages through one P6 catalog until the last page is detected.
    /// </summary>
    public sealed class P6PagedCatalogReader
    {
        private readonly IP6RestApi _api;

        public P6PagedCatalogReader(IP6RestApi api)
        {
            _api = api;
        }

        public async Task<int> FetchAllPagesAsync(
            P6EntityKind kind,
            string cookie,
            string projectFilter,
            int pageSize,
            Action<IReadOnlyList<P6BaseRecord>> onPage,
            CancellationToken cancellationToken = default)
        {
            var seen = new HashSet<int>();
            var totalCount = 0;
            var offset = 0;

            while (true)
            {
                var page = await FetchPageAsync(kind, cookie, projectFilter, pageSize, offset, cancellationToken);
                var added = 0;

                foreach (var record in page)
                {
                    if (seen.Add(record.ObjectId))
                    {
                        added++;
                    }
                }

                if (added > 0)
                {
                    onPage(page);
                    totalCount += added;
                }

                if (page.Count < pageSize || added == 0)
                {
                    break;
                }

                offset += pageSize;
            }

            return totalCount;
        }

        public async Task<IReadOnlyList<P6ProjectRecord>> FetchProjectsAsync(
            string cookie,
            CancellationToken cancellationToken = default)
        {
            var projects = new List<P6ProjectRecord>();
            var offset = 0;

            while (true)
            {
                var page = await _api.GetProjectsAsync(
                    cookie,
                    offset: offset,
                    cancellationToken: cancellationToken);

                if (page is not { Count: > 0 })
                {
                    break;
                }

                projects.AddRange(page);
                if (page.Count < IP6RestApi.ProjectSummaryPageSize)
                {
                    break;
                }

                offset += IP6RestApi.ProjectSummaryPageSize;
            }

            return projects;
        }

        private async Task<IReadOnlyList<P6BaseRecord>> FetchPageAsync(
            P6EntityKind kind,
            string cookie,
            string projectFilter,
            int pageSize,
            int offset,
            CancellationToken cancellationToken)
        {
            return kind switch
            {
                P6EntityKind.Projects => await ToRecordsAsync(_api.GetProjectsAsync(
                    cookie,
                    filter: projectFilter,
                    limit: pageSize,
                    offset: offset,
                    cancellationToken: cancellationToken)),
                P6EntityKind.Wbs => await ToRecordsAsync(_api.GetWbsAsync(
                    cookie,
                    filter: projectFilter,
                    limit: pageSize,
                    offset: offset,
                    cancellationToken: cancellationToken)),
                P6EntityKind.Activities => await ToRecordsAsync(_api.GetActivitiesAsync(
                    cookie,
                    filter: projectFilter,
                    limit: pageSize,
                    offset: offset,
                    cancellationToken: cancellationToken)),
                P6EntityKind.Resources => await ToRecordsAsync(_api.GetResourcesAsync(
                    cookie,
                    limit: pageSize,
                    offset: offset,
                    cancellationToken: cancellationToken)),
                P6EntityKind.ResourceAssignments => await ToRecordsAsync(_api.GetResourceAssignmentsAsync(
                    cookie,
                    filter: projectFilter,
                    limit: pageSize,
                    offset: offset,
                    cancellationToken: cancellationToken)),
                P6EntityKind.Relationships => await ToRecordsAsync(_api.GetRelationshipsAsync(
                    cookie,
                    filter: projectFilter,
                    limit: pageSize,
                    offset: offset,
                    cancellationToken: cancellationToken)),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported P6 entity kind.")
            };
        }

        private static async Task<IReadOnlyList<P6BaseRecord>> ToRecordsAsync<TRecord>(Task<List<TRecord>> call)
            where TRecord : P6BaseRecord
        {
            var records = await call;
            return records == null ? [] : records.Cast<P6BaseRecord>().ToArray();
        }
    }
}
