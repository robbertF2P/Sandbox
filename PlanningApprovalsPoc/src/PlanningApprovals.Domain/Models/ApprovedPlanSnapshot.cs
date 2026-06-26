using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Models;

public sealed class ApprovedPlanSnapshot
{
    private ApprovedPlanSnapshot()
    {
    }

    public ApprovedPlanSnapshot(
        ApprovalPublicId publicId,
        ApprovalPublicId decisionPublicId,
        AssignmentId assignmentId,
        PersonId approvedByPersonId,
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

    public ApprovalPublicId PublicId { get; private init; }

    public ApprovalPublicId DecisionPublicId { get; private init; }

    public AssignmentId AssignmentId { get; private init; }

    public PersonId ApprovedByPersonId { get; private init; }

    public DateTimeOffset ApprovedAt { get; private init; }

    public ProgressRevisionRef ProgressRevision { get; private init; } = null!;

    public PlanSnapshot PlanSnapshot { get; private init; } = null!;

    public static ApprovedPlanSnapshot FromApproval(
        ApprovalDecision decision,
        AssignmentApprovalRequest request) =>
        new(
            new ApprovalPublicId(Guid.NewGuid()),
            decision.PublicId,
            request.AssignmentId,
            decision.DecidedByPersonId,
            decision.DecidedAt,
            request.ProgressRevision,
            request.ProposedPlan);
}
