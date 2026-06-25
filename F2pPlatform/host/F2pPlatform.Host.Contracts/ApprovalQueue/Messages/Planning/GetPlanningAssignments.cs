namespace F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Planning;

using F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record PlanningAssignmentRow(
    TaskId TaskId,
    AssignmentId AssignmentId,
    OrganisationId OrganisationId,
    AssignmentLabels Labels,
    bool IsActiveAssignment);

public sealed record GetPlanningAssignments(ApprovalQueueFilter Filter);

public sealed record GetPlanningAssignmentsReply(IReadOnlyList<PlanningAssignmentRow> Rows);
