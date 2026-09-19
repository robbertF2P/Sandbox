using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Infrastructure.Akka.Actors.Workers;
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

        private P6EntityKind _kind;
        private int _totalCount;

        public P6CatalogWorkerActor(IP6RestApi api, IActorRef store, int pageSize, string projectFilter = null)
        {
            _api = api;
            _store = store;
            _pageSize = pageSize;
            _projectFilter = projectFilter;

            Receive<FetchCatalog>(StartFetch);

            Receive<CatalogFetched>(message =>
            {
                Context.Parent.Tell(message);
                Context.Stop(Self);
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

        private void StartFetch(FetchCatalog message)
        {
            _kind = message.Kind;
            _totalCount = 0;
            _log.Debug("P6 catalog worker fetching {0}", message.Kind);

            var paging = Context.ActorOf(PagedFetchActor<P6BaseRecord>.Props(new PagedFetchOptions<P6BaseRecord>
            {
                PageSize = _pageSize,
                DedupeKey = record => record.ObjectId,
                FetchPage = offset => FetchPageAsync(message.Kind, message.Cookie, offset),
                OnItemsAdded = items =>
                {
                    _store.Tell(new P6RawDataStoreActor.AppendRawData(_kind, items));
                    _totalCount += items.Count;
                },
                BuildSuccessReply = _ => new CatalogFetched(_kind, _totalCount),
                BuildFailureReply = exception => new CatalogFetchFailed(_kind, exception)
            }));

            paging.Tell(new PagedFetchActor<P6BaseRecord>.Start(Self));
        }

        private async Task<IReadOnlyList<P6BaseRecord>> FetchPageAsync(P6EntityKind kind, string cookie, int offset)
        {
            return kind switch
            {
                P6EntityKind.Projects => await ToRecordsAsync(_api.GetProjectsAsync(
                    cookie,
                    filter: _projectFilter,
                    limit: _pageSize,
                    offset: offset,
                    cancellationToken: CancellationToken.None)),
                P6EntityKind.Wbs => await ToRecordsAsync(_api.GetWbsAsync(
                    cookie,
                    filter: _projectFilter,
                    limit: _pageSize,
                    offset: offset,
                    cancellationToken: CancellationToken.None)),
                P6EntityKind.Activities => await ToRecordsAsync(_api.GetActivitiesAsync(
                    cookie,
                    filter: _projectFilter,
                    limit: _pageSize,
                    offset: offset,
                    cancellationToken: CancellationToken.None)),
                P6EntityKind.Resources => await ToRecordsAsync(_api.GetResourcesAsync(
                    cookie,
                    limit: _pageSize,
                    offset: offset,
                    cancellationToken: CancellationToken.None)),
                P6EntityKind.ResourceAssignments => await ToRecordsAsync(_api.GetResourceAssignmentsAsync(
                    cookie,
                    filter: _projectFilter,
                    limit: _pageSize,
                    offset: offset,
                    cancellationToken: CancellationToken.None)),
                P6EntityKind.Relationships => await ToRecordsAsync(_api.GetRelationshipsAsync(
                    cookie,
                    filter: _projectFilter,
                    limit: _pageSize,
                    offset: offset,
                    cancellationToken: CancellationToken.None)),
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
