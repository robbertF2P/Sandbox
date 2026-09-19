using Akka.Actor;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Messages;
using Infrastructure.Akka.Actors.Workers;
using System.Threading;

namespace Floor2Plan.Connectors.P6.Actors
{
    /// <summary>
    /// Pages through the P6 project summary endpoint and returns the full catalog.
    /// </summary>
    public static class P6ProjectCatalogActor
    {
        public static Props Props(IP6RestApi api, string cookie)
        {
            return PagedFetchActor<P6ProjectRecord>.Props(new PagedFetchOptions<P6ProjectRecord>
            {
                PageSize = IP6RestApi.ProjectSummaryPageSize,
                FetchPage = async offset => await api.GetProjectsAsync(
                    cookie,
                    fields: IP6RestApi.ProjectSummaryFields,
                    orderBy: IP6RestApi.ProjectSummaryOrderBy,
                    filter: IP6RestApi.ProjectSummaryFilter,
                    limit: IP6RestApi.ProjectSummaryPageSize,
                    offset: offset,
                    cancellationToken: CancellationToken.None),
                BuildSuccessReply = projects => new P6Projects(projects)
            });
        }
    }
}
