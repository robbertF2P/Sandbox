using Floor2Plan.Connectors.P6.Sync;
using System.Collections.Generic;

namespace Floor2Plan.Connectors.P6.Messages
{
    public sealed record StartP6Sync(
        IReadOnlyList<string> ProjectIds,
        IReadOnlyList<P6ProjectSyncPlan> SyncPlans = null);
}
