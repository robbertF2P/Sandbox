using F2pPlatform.Host.Contracts.ApprovalQueue;
using Platform.Shared.Domain;

namespace F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Planning;

public sealed record PlanningAssignmentRow(
    TaskId TaskId,
    AssignmentId AssignmentId,
    OrganisationId OrganisationId,
    AssignmentLabels Labels,
    bool IsActiveAssignment);

public sealed record GetPlanningAssignments(ApprovalQueueFilter Filter);

public sealed record GetPlanningAssignmentsReply(IReadOnlyList<PlanningAssignmentRow> Rows);
