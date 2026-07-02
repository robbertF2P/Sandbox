using HourApprovals.Domain.Enums;
using HourApprovals.Domain.Models;
using HourApprovals.Domain.ValueObjects;

namespace HourApprovals.Domain.Rules;

public static class HourApprovalRules
{
    public const string ApproveHoursProgressPermission = "ApproveHoursProgress";

    public static DateOnly ResolveApprovalDay(DateTimeOffset approvedAtUtc) =>
        DateOnly.FromDateTime(approvedAtUtc.UtcDateTime);

    public static TaskApprovalState ResolveState(
        ApprovalValues currentValues,
        ApprovalRecord? lastApproval) =>
        lastApproval is not null && currentValues.Matches(lastApproval.ApprovedValues)
            ? TaskApprovalState.Approved
            : TaskApprovalState.NotApproved;

    public static bool MatchesFilter(TaskApprovalState state, ApprovalFilterStatus filter) =>
        filter switch
        {
            ApprovalFilterStatus.All => true,
            ApprovalFilterStatus.Approved => state == TaskApprovalState.Approved,
            ApprovalFilterStatus.NotApproved => state == TaskApprovalState.NotApproved,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, "Unsupported filter."),
        };

    public static bool CanApprove(IReadOnlyCollection<string> permissions) =>
        permissions.Contains(ApproveHoursProgressPermission, StringComparer.Ordinal);
}
