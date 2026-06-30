using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Rules;

public static class PlanningApprovalRules
{
    public static DateOnly ResolveApprovalDay(DateTimeOffset approvedAtUtc) =>
        DateOnly.FromDateTime(approvedAtUtc.UtcDateTime);

    public static AssignmentApprovalState ResolveState(
        ApprovalValues currentValues,
        AssignmentApprovalRecord? lastApproval) =>
        lastApproval is not null && currentValues.Matches(lastApproval.ApprovedValues)
            ? AssignmentApprovalState.Approved
            : AssignmentApprovalState.NotApproved;
}
