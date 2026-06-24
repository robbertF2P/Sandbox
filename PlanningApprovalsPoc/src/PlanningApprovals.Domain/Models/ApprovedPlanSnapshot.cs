using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Models;

public sealed class ApprovedPlanSnapshot
{
    private ApprovedPlanSnapshot()
    {
    }

    public ApprovedPlanSnapshot(
        Guid publicId,
        Guid decisionPublicId,
        long assignmentId,
        long approvedByPersonId,
        DateTimeOffset approvedAt,
        ProgressRevisionRef progressRevision,
        PlanSnapshot planSnapshot)
    {
        PublicId = publicId;
        DecisionPublicId = decisionPublicId;
        AssignmentId = assignmentId;
        ApprovedByPersonId = approvedByPersonId;
        ApprovedAt = approvedAt;
        ProgressRevision = progressRevision;
        PlanSnapshot = planSnapshot;
    }

    public int Id { get; private set; }

    public Guid PublicId { get; private init; }

    public Guid DecisionPublicId { get; private init; }

    public long AssignmentId { get; private init; }

    public long ApprovedByPersonId { get; private init; }

    public DateTimeOffset ApprovedAt { get; private init; }

    public ProgressRevisionRef ProgressRevision { get; private init; } = null!;

    public PlanSnapshot PlanSnapshot { get; private init; } = null!;

    public static ApprovedPlanSnapshot FromApproval(
        ApprovalDecision decision,
        AssignmentApprovalRequest request) =>
        new(
            Guid.NewGuid(),
            decision.PublicId,
            request.AssignmentId,
            decision.DecidedByPersonId,
            decision.DecidedAt,
            request.ProgressRevision,
            request.ProposedPlan);
}
