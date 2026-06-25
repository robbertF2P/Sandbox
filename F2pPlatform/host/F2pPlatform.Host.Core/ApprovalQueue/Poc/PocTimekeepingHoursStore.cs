using F2pPlatform.Host.Contracts.ApprovalQueue;
using Platform.Shared.Domain;

namespace F2pPlatform.Host.Core.ApprovalQueue.Poc;

/// <summary>
/// POC stand-in for Timekeeping module. Production: Timekeeping read actor + Timekeeping DB only.
/// </summary>
internal static class PocTimekeepingHoursStore
{
    private static readonly IReadOnlyDictionary<AssignmentId, decimal> HoursInWindow =
        new Dictionary<AssignmentId, decimal>
        {
            [new AssignmentId(Guid.Parse("11111111-1111-1111-1111-111111111101"))] = 4.5m,
            [new AssignmentId(Guid.Parse("11111111-1111-1111-1111-111111111102"))] = 0m,
            [new AssignmentId(Guid.Parse("11111111-1111-1111-1111-111111111103"))] = 2m,
        };

    public static IReadOnlyDictionary<AssignmentId, decimal> GetHours(IReadOnlyList<AssignmentId> assignmentIds)
    {
        Dictionary<AssignmentId, decimal> result = new();
        foreach (AssignmentId id in assignmentIds)
        {
            if (HoursInWindow.TryGetValue(id, out decimal hours))
            {
                result[id] = hours;
            }
        }

        return result;
    }
}
