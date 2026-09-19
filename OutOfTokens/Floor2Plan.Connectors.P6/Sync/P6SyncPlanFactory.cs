using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace Floor2Plan.Connectors.P6.Sync
{
    public static class P6SyncPlanFactory
    {
        public static IReadOnlyList<P6ProjectSyncPlan> FromSyncOptions(
            IReadOnlyList<string> projectObjectIds,
            P6SyncOptions syncOptions)
        {
            if (!RequiresExplicitPlans(syncOptions))
            {
                return null;
            }

            var entityKinds = syncOptions.EntityKinds?.ToHashSet();
            return projectObjectIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .Select(id => new P6ProjectSyncPlan(id, entityKinds, syncOptions.AdditionalFilter))
                .ToList();
        }

        public static bool RequiresExplicitPlans(P6SyncOptions syncOptions)
        {
            return syncOptions.EntityKinds is { Length: > 0 }
                || !string.IsNullOrWhiteSpace(syncOptions.AdditionalFilter);
        }
    }
}
