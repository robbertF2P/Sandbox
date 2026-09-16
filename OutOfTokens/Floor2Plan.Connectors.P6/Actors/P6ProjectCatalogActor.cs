using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Floor2Plan.Connectors.P6.Actors;

/// <summary>
/// Pages through the P6 project summary endpoint and returns the full catalog.
/// </summary>
public sealed class P6ProjectCatalogActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IP6RestApi _api;

    public P6ProjectCatalogActor(IP6RestApi api)
    {
        _api = api;

        Receive<Fetch>(message =>
        {
            _log.Info("Fetching P6 project catalog");
            FetchPage(message.Cookie, message.ReplyTo, offset: 0, accumulated: []);
        });

        Receive<PageReceived>(message =>
        {
            var projects = message.Accumulated.Concat(message.Page).ToList();
            if (message.Page.Count < IP6RestApi.ProjectSummaryPageSize)
            {
                message.ReplyTo.Tell(new P6Projects(projects));
                Context.Stop(Self);
                return;
            }

            FetchPage(
                message.Cookie,
                message.ReplyTo,
                message.Offset + IP6RestApi.ProjectSummaryPageSize,
                projects);
        });

        Receive<PageFailed>(message =>
        {
            _log.Error(message.Exception, "Failed to retrieve P6 project catalog");
            message.ReplyTo.Tell(new Status.Failure(message.Exception));
            Context.Stop(Self);
        });
    }

    public static Props Props(IP6RestApi api)
    {
        return Akka.Actor.Props.Create(() => new P6ProjectCatalogActor(api));
    }

    internal sealed record Fetch(string Cookie, IActorRef ReplyTo);

    private sealed record PageReceived(
        string Cookie,
        IActorRef ReplyTo,
        int Offset,
        IList<P6ProjectRecord> Accumulated,
        List<P6ProjectRecord> Page);

    private sealed record PageFailed(IActorRef ReplyTo, Exception Exception);

    private void FetchPage(string cookie, IActorRef replyTo, int offset, IList<P6ProjectRecord> accumulated)
    {
        _ = _api.GetProjectsAsync(
                cookie,
                fields: IP6RestApi.ProjectSummaryFields,
                orderBy: IP6RestApi.ProjectSummaryOrderBy,
                filter: IP6RestApi.ProjectSummaryFilter,
                limit: IP6RestApi.ProjectSummaryPageSize,
                offset: offset,
                cancellationToken: CancellationToken.None)
            .PipeTo(
                Self,
                Self,
                page => new PageReceived(cookie, replyTo, offset, accumulated, page),
                exception => new PageFailed(replyTo, exception));
    }
}
