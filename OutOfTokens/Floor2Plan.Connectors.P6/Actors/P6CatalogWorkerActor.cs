using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Floor2Plan.Connectors.P6.Actors
{
    public sealed class P6CatalogWorkerActor : ReceiveActor
    {
        private readonly IP6RestApi _api;
        private readonly IActorRef _store;
        private readonly int _pageSize;
        private readonly string _projectFilter;
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private int _totalCount;
        private HashSet<int> _seenObjectIds = new();

        public P6CatalogWorkerActor(IP6RestApi api, IActorRef store, int pageSize, string projectFilter = null)
        {
            _api = api;
            _store = store;
            _pageSize = pageSize;
            _projectFilter = projectFilter;

            Receive<FetchCatalog>(message =>
            {
                _log.Debug("P6 catalog worker fetching {0} page at offset {1}", message.Kind, message.Offset);
                FetchPage(message);
            });

            Receive<CatalogPageFetched>(message =>
            {
                var newRecords = message.Records.Where(x => _seenObjectIds.Add(x.ObjectId)).ToArray();
                _store.Tell(new P6RawDataStoreActor.AppendRawData(message.Kind, newRecords));
                _totalCount += newRecords.Length;

                // The live P6 REST server does not honor Limit/Offset for every endpoint - it can return
                // the full result set on every request regardless of paging parameters. If a page didn't
                // introduce any records we haven't already seen, treat the catalog as fully fetched instead
                // of looping forever on Offset increments the server ignores.
                var isLastPage = message.Records.Count < _pageSize || newRecords.Length == 0;
                if (isLastPage)
                {
                    Context.Parent.Tell(new CatalogFetched(message.Kind, _totalCount));
                    Context.Stop(Self);
                    return;
                }

                FetchPage(new FetchCatalog(message.Kind, message.Cookie, message.Offset + _pageSize));
            });

            Receive<CatalogFetchFailed>(message =>
            {
                Context.Parent.Tell(message);
                Context.Stop(Self);
            });
        }

        public static Props Props(IP6RestApi api, IActorRef store, int pageSize, string projectFilter = null)
        {
            return Akka.Actor.Props.Create(() => new P6CatalogWorkerActor(api, store, pageSize, projectFilter));
        }

        internal sealed record FetchCatalog(P6EntityKind Kind, string Cookie, int Offset);

        internal sealed record CatalogFetched(P6EntityKind Kind, int Count);

        internal sealed record CatalogFetchFailed(P6EntityKind Kind, Exception Exception);

        private sealed record CatalogPageFetched(
            P6EntityKind Kind,
            string Cookie,
            int Offset,
            IReadOnlyList<P6BaseRecord> Records);

        private void FetchPage(FetchCatalog message)
        {
            _ = FetchPageAsync(message).PipeTo(
                Self,
                Self,
                records => new CatalogPageFetched(message.Kind, message.Cookie, message.Offset, records),
                exception => new CatalogFetchFailed(message.Kind, exception));
        }

        private async Task<IReadOnlyList<P6BaseRecord>> FetchPageAsync(FetchCatalog message)
        {
            return message.Kind switch
            {
                P6EntityKind.Projects => await ToRecordsAsync(_api.GetProjectsAsync(
                    message.Cookie,
                    filter: _projectFilter,
                    limit: _pageSize,
                    offset: message.Offset,
                    cancellationToken: CancellationToken.None)),
                P6EntityKind.Wbs => await ToRecordsAsync(_api.GetWbsAsync(
                    message.Cookie,
                    filter: _projectFilter,
                    limit: _pageSize,
                    offset: message.Offset,
                    cancellationToken: CancellationToken.None)),
                P6EntityKind.Activities => await ToRecordsAsync(_api.GetActivitiesAsync(
                    message.Cookie,
                    filter: _projectFilter,
                    limit: _pageSize,
                    offset: message.Offset,
                    cancellationToken: CancellationToken.None)),
                P6EntityKind.Resources => await ToRecordsAsync(_api.GetResourcesAsync(
                    message.Cookie,
                    limit: _pageSize,
                    offset: message.Offset,
                    cancellationToken: CancellationToken.None)),
                P6EntityKind.ResourceAssignments => await ToRecordsAsync(_api.GetResourceAssignmentsAsync(
                    message.Cookie,
                    filter: _projectFilter,
                    limit: _pageSize,
                    offset: message.Offset,
                    cancellationToken: CancellationToken.None)),
                P6EntityKind.Relationships => await ToRecordsAsync(_api.GetRelationshipsAsync(
                    message.Cookie,
                    filter: _projectFilter,
                    limit: _pageSize,
                    offset: message.Offset,
                    cancellationToken: CancellationToken.None)),
                _ => throw new ArgumentOutOfRangeException(nameof(message), message.Kind, "Unsupported P6 entity kind.")
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
