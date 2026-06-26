using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Models;

public sealed class ApprovalDecision
{
    private ApprovalDecision()
    {
    }

    public ApprovalDecision(
        ApprovalPublicId publicId,
        ApprovalPublicId requestPublicId,
        AssignmentId assignmentId,
        ApprovalDecisionType decision,
        PersonId decidedByPersonId,
        DateTimeOffset decidedAt,
        ProgressRevisionRef progressRevisionAtDecision,
        PlanSnapshot proposedPlanAtDecision,
        DecisionComment? comment,
        CorrelationId correlationId,
        ApprovalPublicId? batchPublicId)
    {
        PublicId = publicId;
        RequestPublicId = requestPublicId;
        AssignmentId = assignmentId;
        Decision = decision;
        DecidedByPersonId = decidedByPersonId;
        DecidedAt = decidedAt;
        ProgressRevisionAtDecision = progressRevisionAtDecision;
        ProposedPlanAtDecision = proposedPlanAtDecision;
        Comment = comment;
        CorrelationId = correlationId;
        BatchPublicId = batchPublicId;
    }

    public int Id { get; private set; }

    public ApprovalPublicId PublicId { get; private init; }

    public ApprovalPublicId RequestPublicId { get; private init; }

    public AssignmentId AssignmentId { get; private init; }

    public ApprovalDecisionType Decision { get; private init; }

    public PersonId DecidedByPersonId { get; private init; }

    public DateTimeOffset DecidedAt { get; private init; }

    public ProgressRevisionRef ProgressRevisionAtDecision { get; private init; } = null!;

    public PlanSnapshot ProposedPlanAtDecision { get; private init; } = null!;

    public DecisionComment? Comment { get; private init; }

    public CorrelationId CorrelationId { get; private init; }

    public ApprovalPublicId? BatchPublicId { get; private init; }

    public static ApprovalDecision Record(
        AssignmentApprovalRequest request,
        ApprovalDecisionType decision,
        PersonId decidedByPersonId,
        DateTimeOffset decidedAt,
        DecisionComment? comment,
        CorrelationId correlationId,
        ApprovalPublicId? batchPublicId) =>
        new(
            new ApprovalPublicId(Guid.NewGuid()),
            request.PublicId,
            request.AssignmentId,
            decision,
            decidedByPersonId,
            decidedAt,
            request.ProgressRevision,
            request.ProposedPlan,
            comment,
            correlationId,
            batchPublicId);
}
