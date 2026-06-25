namespace F2pPlatform.Host.Core.ApprovalQueue.Poc;

/// <summary>
/// POC stand-in for Timekeeping module. Production: Timekeeping read actor + Timekeeping DB only.
/// </summary>
internal static class PocTimekeepingHoursStore
{
    private static readonly IReadOnlyDictionary<Guid, decimal> HoursInWindow = new Dictionary<Guid, decimal>
    {
        [Guid.Parse("11111111-1111-1111-1111-111111111101")] = 4.5m,
        [Guid.Parse("11111111-1111-1111-1111-111111111102")] = 0m,
        [Guid.Parse("11111111-1111-1111-1111-111111111103")] = 2m,
    };

    public static IReadOnlyDictionary<Guid, decimal> GetHours(IReadOnlyList<Guid> assignmentIds)
    {
        Dictionary<Guid, decimal> result = new();
        foreach (Guid id in assignmentIds)
        {
            if (HoursInWindow.TryGetValue(id, out decimal hours))
            {
                result[id] = hours;
            }
        }

        return result;
    }
}
