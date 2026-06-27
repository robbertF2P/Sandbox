using F2pPlatform.Host.Contracts.ApprovalQueue;
using Platform.Shared.Domain;

namespace F2pPlatform.Host.Core.ApprovalQueue.Poc;

/// <summary>
/// POC stand-in for Timekeeping module. Production: Timekeeping read actor + Timekeeping DB only.
/// </summary>
internal static class PocTimekeepingHoursStore
{
    private static readonly IReadOnlyDictionary<AssignmentId, IReadOnlyDictionary<TimeRangePreset, decimal>> HoursByPreset =
        new Dictionary<AssignmentId, IReadOnlyDictionary<TimeRangePreset, decimal>>
        {
            [new AssignmentId(Guid.Parse("11111111-1111-1111-1111-111111111101"))] =
                new Dictionary<TimeRangePreset, decimal>
                {
                    [TimeRangePreset.SinceLastSubmission] = 4.5m,
                    [TimeRangePreset.LastWeek] = 6m,
                    [TimeRangePreset.CurrentWeek] = 2m,
                    [TimeRangePreset.Custom] = 4.5m,
                },
            [new AssignmentId(Guid.Parse("11111111-1111-1111-1111-111111111102"))] =
                new Dictionary<TimeRangePreset, decimal>
                {
                    [TimeRangePreset.SinceLastSubmission] = 0m,
                    [TimeRangePreset.LastWeek] = 1m,
                    [TimeRangePreset.CurrentWeek] = 0m,
                    [TimeRangePreset.Custom] = 0m,
                },
            [new AssignmentId(Guid.Parse("11111111-1111-1111-1111-111111111103"))] =
                new Dictionary<TimeRangePreset, decimal>
                {
                    [TimeRangePreset.SinceLastSubmission] = 2m,
                    [TimeRangePreset.LastWeek] = 0m,
                    [TimeRangePreset.CurrentWeek] = 3m,
                    [TimeRangePreset.Custom] = 2m,
                },
        };

    public static IReadOnlyDictionary<AssignmentId, decimal> GetHours(
        IReadOnlyList<AssignmentId> assignmentIds,
        TimeRangePreset timeRangePreset)
    {
        Dictionary<AssignmentId, decimal> result = new();
        foreach (AssignmentId id in assignmentIds)
        {
            if (HoursByPreset.TryGetValue(id, out IReadOnlyDictionary<TimeRangePreset, decimal>? byPreset)
                && byPreset.TryGetValue(timeRangePreset, out decimal hours))
            {
                result[id] = hours;
            }
        }

        return result;
    }
}
