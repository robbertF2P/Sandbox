using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Models;

public sealed class AssignmentApprovalRequest
{
    private AssignmentApprovalRequest()
    {
    }

    public AssignmentApprovalRequest(
        Guid publicId,
        long projectId,
        long assignmentId,
        ApprovalRequiredBecause requiredBecause,
        ProgressRevisionRef progressRevision,
        PlanSnapshot proposedPlan,
        Guid? lastApprovedSnapshotId,
        DateTimeOffset openedAt,
        string openedByProcess)
    {
        if (string.IsNullOrWhiteSpace(openedByProcess))
        {
            throw new ArgumentException("Opened-by process is required.", nameof(openedByProcess));
        }

        PublicId = publicId;
        ProjectId = projectId;
        AssignmentId = assignmentId;
        Status = ApprovalRequestStatus.Pending;
        RequiredBecause = requiredBecause;
        ProgressRevision = progressRevision;
        ProposedPlan = proposedPlan;
        LastApprovedSnapshotId = lastApprovedSnapshotId;
        OpenedAt = openedAt;
        OpenedByProcess = openedByProcess;
    }

    public int Id { get; private set; }

    public Guid PublicId { get; private init; }

    public long ProjectId { get; private init; }

    public long AssignmentId { get; private init; }

    public ApprovalRequestStatus Status { get; private set; }

    public ApprovalRequiredBecause RequiredBecause { get; private init; }

    public ProgressRevisionRef ProgressRevision { get; private init; } = null!;

    public PlanSnapshot ProposedPlan { get; private init; } = null!;

    public Guid? LastApprovedSnapshotId { get; private init; }

    public DateTimeOffset OpenedAt { get; private init; }

    public string OpenedByProcess { get; private init; } = string.Empty;

    public DateTimeOffset? ClosedAt { get; private set; }

    public static AssignmentApprovalRequest Open(
        long projectId,
        long assignmentId,
        ApprovalRequiredBecause requiredBecause,
        ProgressRevisionRef progressRevision,
        PlanSnapshot proposedPlan,
        ApprovedPlanSnapshot? lastApproved,
        DateTimeOffset openedAt,
        string openedByProcess) =>
        new(
            Guid.NewGuid(),
            projectId,
            assignmentId,
            requiredBecause,
            progressRevision,
            proposedPlan,
            lastApproved?.PublicId,
            openedAt,
            openedByProcess);

    public void MarkSuperseded(DateTimeOffset closedAt)
    {
        if (Status != ApprovalRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Only pending requests can be superseded. Current status: {Status}.");
        }

        Status = ApprovalRequestStatus.Superseded;
        ClosedAt = closedAt;
    }

    public void MarkApproved(DateTimeOffset closedAt)
    {
        EnsurePending();
        Status = ApprovalRequestStatus.Approved;
        ClosedAt = closedAt;
    }

    public void MarkRejected(DateTimeOffset closedAt)
    {
        EnsurePending();
        Status = ApprovalRequestStatus.Rejected;
        ClosedAt = closedAt;
    }

    private void EnsurePending()
    {
        if (Status != ApprovalRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Only pending requests accept decisions. Current status: {Status}.");
        }
    }
}
