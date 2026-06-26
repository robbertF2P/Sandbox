using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;

namespace PlanningApprovals.Domain.Services;

public sealed record ApprovalSyncAction(
    ApprovalSyncActionKind Kind,
    AssignmentApprovalRequest? SupersededRequest = null,
    AssignmentApprovalRequest? OpenedRequest = null)
{
    public static ApprovalSyncAction None { get; } = new(ApprovalSyncActionKind.None);

    public static ApprovalSyncAction Supersede(AssignmentApprovalRequest request) =>
        new(ApprovalSyncActionKind.SupersedePendingRequest, request);

    public static ApprovalSyncAction Open(AssignmentApprovalRequest request) =>
        new(ApprovalSyncActionKind.OpenRequest, OpenedRequest: request);
}
