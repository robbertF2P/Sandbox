using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Models;

public sealed class AssignmentApprovalRequest
{
    private AssignmentApprovalRequest()
    {
    }

    public AssignmentApprovalRequest(
        ApprovalPublicId publicId,
        ProjectId projectId,
        AssignmentId assignmentId,
        ApprovalRequiredBecause requiredBecause,
        ProgressRevisionRef progressRevision,
        PlanSnapshot proposedPlan,
        PlanningStateSnapshot lookbackBaseline,
        ApprovalPublicId? lastApprovedSnapshotId,
        DateTimeOffset openedAt,
        ProcessName openedByProcess)
    {
        ArgumentNullException.ThrowIfNull(lookbackBaseline);

        PublicId = publicId;
        ProjectId = projectId;
        AssignmentId = assignmentId;
        Status = ApprovalRequestStatus.Pending;
        RequiredBecause = requiredBecause;
        ProgressRevision = progressRevision;
        ProposedPlan = proposedPlan;
        LookbackCapturedAt = lookbackBaseline.CapturedAt;
        LookbackProgress = lookbackBaseline.ProgressRevision;
        LookbackPlan = lookbackBaseline.PlanSnapshot;
        LastApprovedSnapshotId = lastApprovedSnapshotId;
        OpenedAt = openedAt;
        OpenedByProcess = openedByProcess;
    }

    public int Id { get; private set; }

    public ApprovalPublicId PublicId { get; private init; }

    public ProjectId ProjectId { get; private init; }

    public AssignmentId AssignmentId { get; private init; }

    public ApprovalRequestStatus Status { get; private set; }

    public ApprovalRequiredBecause RequiredBecause { get; private init; }

    public ProgressRevisionRef ProgressRevision { get; private init; } = null!;

    public PlanSnapshot ProposedPlan { get; private init; } = null!;

    public DateTimeOffset LookbackCapturedAt { get; private init; }

    public ProgressRevisionRef LookbackProgress { get; private init; } = null!;

    public PlanSnapshot LookbackPlan { get; private init; } = null!;

    public PlanningStateSnapshot LookbackBaseline =>
        new(LookbackCapturedAt, LookbackProgress, LookbackPlan);

    public ApprovalPublicId? LastApprovedSnapshotId { get; private init; }

    public DateTimeOffset OpenedAt { get; private init; }

    public ProcessName OpenedByProcess { get; private init; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public static AssignmentApprovalRequest Open(
        ProjectId projectId,
        AssignmentId assignmentId,
        ApprovalRequiredBecause requiredBecause,
        ProgressRevisionRef progressRevision,
        PlanSnapshot proposedPlan,
        PlanningStateSnapshot lookbackBaseline,
        ApprovedPlanSnapshot? lastApproved,
        DateTimeOffset openedAt,
        ProcessName openedByProcess) =>
        new(
            new ApprovalPublicId(Guid.NewGuid()),
            projectId,
            assignmentId,
            requiredBecause,
            progressRevision,
            proposedPlan,
            lookbackBaseline,
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
