using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Floor2Plan.Connectors.P6.Actors
{
    /// <summary>
    /// Pages through the P6 project summary endpoint and returns the full catalog.
    /// </summary>
    public sealed class P6ProjectCatalogActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IP6RestApi _api;

        private string _cookie = string.Empty;
        private IActorRef _replyTo = ActorRefs.Nobody;

        public P6ProjectCatalogActor(IP6RestApi api)
        {
            _api = api;
            Receive<Fetch>(StartFetch);
        }

        public static Props Props(IP6RestApi api)
        {
            return Akka.Actor.Props.Create(() => new P6ProjectCatalogActor(api));
        }

        internal sealed record Fetch(string Cookie, IActorRef ReplyTo);

        private sealed record PageReceived(int Offset, IList<P6ProjectRecord> Accumulated, List<P6ProjectRecord> Page);

        private sealed record PageFailed(Exception Exception);

        private void StartFetch(Fetch message)
        {
            _cookie = message.Cookie;
            _replyTo = message.ReplyTo;
            _log.Info("Fetching P6 project catalog");
            Become(Paging);
            FetchPage(offset: 0, accumulated: []);
        }

        private void Paging()
        {
            Receive<PageReceived>(message =>
            {
                var projects = message.Accumulated.Concat(message.Page).ToList();
                if (message.Page.Count < IP6RestApi.ProjectSummaryPageSize)
                {
                    _replyTo.Tell(new P6Projects(projects));
                    Context.Stop(Self);
                    return;
                }

                FetchPage(message.Offset + IP6RestApi.ProjectSummaryPageSize, projects);
            });

            Receive<PageFailed>(message =>
            {
                _log.Error(message.Exception, "Failed to retrieve P6 project catalog");
                _replyTo.Tell(new Status.Failure(message.Exception));
                Context.Stop(Self);
            });
        }

        private void FetchPage(int offset, IList<P6ProjectRecord> accumulated)
        {
            _ = _api.GetProjectsAsync(
                    _cookie,
                    fields: IP6RestApi.ProjectSummaryFields,
                    orderBy: IP6RestApi.ProjectSummaryOrderBy,
                    filter: IP6RestApi.ProjectSummaryFilter,
                    limit: IP6RestApi.ProjectSummaryPageSize,
                    offset: offset,
                    cancellationToken: CancellationToken.None)
                .PipeTo(
                    Self,
                    Self,
                    page => new PageReceived(offset, accumulated, page),
                    exception => new PageFailed(exception));
        }
    }
}
