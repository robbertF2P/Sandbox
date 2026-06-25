using F2pPlatform.Host.Contracts.ApprovalQueue;

namespace F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Planning;

public sealed record PlanningAssignmentRow(
    Guid TaskId,
    Guid AssignmentId,
    int OrganisationId,
    string OrganisationLabel,
    string ProjectLabel,
    string Title,
    string ActivityCode,
    bool IsActiveAssignment);

public sealed record GetPlanningAssignments(ApprovalQueueFilter Filter);

public sealed record GetPlanningAssignmentsReply(IReadOnlyList<PlanningAssignmentRow> Rows);
