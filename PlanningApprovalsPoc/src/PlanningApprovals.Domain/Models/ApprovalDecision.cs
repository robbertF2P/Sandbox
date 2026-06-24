using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Models;

public sealed class ApprovalDecision
{
    private ApprovalDecision()
    {
    }

    public ApprovalDecision(
        Guid publicId,
        Guid requestPublicId,
        long assignmentId,
        ApprovalDecisionType decision,
        long decidedByPersonId,
        DateTimeOffset decidedAt,
        ProgressRevisionRef progressRevisionAtDecision,
        PlanSnapshot proposedPlanAtDecision,
        string? comment,
        string correlationId,
        Guid? batchPublicId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("Correlation id is required.", nameof(correlationId));
        }

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

    public Guid PublicId { get; private init; }

    public Guid RequestPublicId { get; private init; }

    public long AssignmentId { get; private init; }

    public ApprovalDecisionType Decision { get; private init; }

    public long DecidedByPersonId { get; private init; }

    public DateTimeOffset DecidedAt { get; private init; }

    public ProgressRevisionRef ProgressRevisionAtDecision { get; private init; } = null!;

    public PlanSnapshot ProposedPlanAtDecision { get; private init; } = null!;

    public string? Comment { get; private init; }

    public string CorrelationId { get; private init; } = string.Empty;

    public Guid? BatchPublicId { get; private init; }

    public static ApprovalDecision Record(
        AssignmentApprovalRequest request,
        ApprovalDecisionType decision,
        long decidedByPersonId,
        DateTimeOffset decidedAt,
        string? comment,
        string correlationId,
        Guid? batchPublicId) =>
        new(
            Guid.NewGuid(),
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
