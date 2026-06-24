using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.Services;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Tests.Support;

public static class Floor2PlanApprovalScenario
{
    public const long ProjectId = 4721;
    public const long ForemanPersonId = 9001;
    public const long AssignmentWelding = 101;
    public const long AssignmentFitting = 102;

    public static ProgressRevisionRef Progress(
        long assignmentId,
        long revisionId,
        decimal percentComplete,
        decimal bookedHours,
        DateTimeOffset recordedAt,
        string source = "Timesheet") =>
        new(assignmentId, revisionId, recordedAt, percentComplete, bookedHours, source);

    public static PlanSnapshot Plan(
        DateOnly start,
        DateOnly finish,
        decimal hours,
        string profileFingerprint,
        string calculationRunId) =>
        new(start, finish, hours, profileFingerprint, calculationRunId);

    public static ApprovalSyncResult ApplyPlanningChange(
        long assignmentId,
        ProgressRevisionRef progress,
        PlanSnapshot proposedPlan,
        AssignmentApprovalRequest? openPending,
        ApprovedPlanSnapshot? lastApproved,
        DateTimeOffset occurredAt) =>
        PlanningApprovalCoordinator.SynchronizeAfterPlanningChange(
            ProjectId,
            assignmentId,
            progress,
            proposedPlan,
            openPending,
            lastApproved,
            occurredAt,
            "planning-recalculation");

    public static ForemanDecisionResult Approve(
        AssignmentApprovalRequest request,
        DateTimeOffset decidedAt,
        string correlationId = "approve-use-case") =>
        PlanningApprovalCoordinator.RecordForemanDecision(
            request,
            ApprovalDecisionType.Approved,
            ForemanPersonId,
            decidedAt,
            comment: null,
            correlationId,
            batchPublicId: null);
}
