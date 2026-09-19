using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Sync;

namespace Floor2Plan.Connectors.P6.Actors
{
    internal sealed record P6CatalogSyncWorkItem(
        P6ProjectSyncPlan Plan,
        int ProjectObjectId,
        P6EntityKind Kind,
        string Cookie,
        string ProjectFilter);
}
