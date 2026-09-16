using System.Collections.Generic;

namespace Floor2Plan.Connectors.P6.Messages
{
    public sealed record P6SyncResult(
        int ProjectCount,
        int WbsCount,
        int ActivityCount,
        int ResourceCount,
        int ResourceAssignmentCount,
        int RelationshipCount,
        IReadOnlyList<string> Errors)
    {
        public bool Succeeded => Errors.Count == 0;

        public int TotalRecordCount =>
            ProjectCount +
            WbsCount +
            ActivityCount +
            ResourceCount +
            ResourceAssignmentCount +
            RelationshipCount;
    }
}
