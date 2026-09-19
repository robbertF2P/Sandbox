using Floor2Plan.Connectors.P6.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Floor2Plan.Connectors.P6.Sync
{
    /// <summary>
    /// Describes what to fetch for one P6 project during a sync run.
    /// </summary>
    public sealed record P6ProjectSyncPlan(
        string ProjectObjectId,
        IReadOnlySet<P6EntityKind> EntityKinds = null,
        string AdditionalFilter = null)
    {
        public static P6ProjectSyncPlan Full(string projectObjectId)
        {
            return new P6ProjectSyncPlan(projectObjectId);
        }

        public IReadOnlySet<P6EntityKind> GetEntityKinds()
        {
            return EntityKinds ?? DefaultEntityKinds;
        }

        public string BuildFilter(P6EntityKind kind, int projectObjectId)
        {
            var projectFilter = kind switch
            {
                P6EntityKind.Projects => $"ObjectId={projectObjectId}",
                P6EntityKind.Relationships => $"PredecessorProjectObjectId={projectObjectId}",
                _ => $"ProjectObjectId={projectObjectId}"
            };

            if (string.IsNullOrWhiteSpace(AdditionalFilter))
            {
                return projectFilter;
            }

            return $"{projectFilter};{AdditionalFilter}";
        }

        private static readonly HashSet<P6EntityKind> DefaultEntityKinds =
            Enum.GetValues<P6EntityKind>().ToHashSet();
    }
}
