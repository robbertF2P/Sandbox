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

    public static readonly DateTimeOffset WeekAgo = new(2026, 6, 17, 8, 0, 0, TimeSpan.Zero);

    public static readonly DateTimeOffset Today = new(2026, 6, 24, 8, 0, 0, TimeSpan.Zero);

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

    public static AssignmentPlanningCheckpoint Checkpoint(
        long assignmentId,
        DateTimeOffset capturedAt,
        ProgressRevisionRef progress,
        PlanSnapshot plan,
        string captureSource = "nightly-capture") =>
        AssignmentPlanningCheckpoint.Capture(assignmentId, capturedAt, progress, plan, captureSource);

    public static PlanningStateSnapshot State(
        DateTimeOffset capturedAt,
        ProgressRevisionRef progress,
        PlanSnapshot plan) =>
        new(capturedAt, progress, plan);

    /// <summary>
    /// Default ~1 week lookback: checkpoint captured 8 days before <paramref name="occurredAt"/>.
    /// </summary>
    public static IReadOnlyList<AssignmentPlanningCheckpoint> DefaultWeekAgoHistory(
        long assignmentId,
        DateTimeOffset occurredAt,
        decimal baselinePercent = 30m,
        decimal baselineHours = 90m,
        DateOnly? planStart = null,
        DateOnly? planFinish = null) =>
    [
        Checkpoint(
            assignmentId,
            occurredAt.AddDays(-8),
            Progress(assignmentId, revisionId: 1, baselinePercent, baselineHours, occurredAt.AddDays(-8)),
            Plan(
                planStart ?? new DateOnly(2026, 6, 1),
                planFinish ?? new DateOnly(2026, 6, 18),
                200m,
                "profile-v1",
                "run-baseline")),
    ];

    public static ApprovalSyncResult ApplyPlanningChange(
        long assignmentId,
        ProgressRevisionRef progress,
        PlanSnapshot proposedPlan,
        IReadOnlyList<AssignmentPlanningCheckpoint> checkpointHistory,
        AssignmentApprovalRequest? openPending,
        ApprovedPlanSnapshot? lastApproved,
        DateTimeOffset occurredAt) =>
        PlanningApprovalCoordinator.SynchronizeAfterPlanningChange(
            ProjectId,
            assignmentId,
            progress,
            proposedPlan,
            checkpointHistory,
            openPending,
            lastApproved,
            occurredAt,
            "planning-recalculation");

    public static ApprovalSyncResult ApplyPlanningChange(
        long assignmentId,
        ProgressRevisionRef progress,
        PlanSnapshot proposedPlan,
        AssignmentApprovalRequest? openPending,
        ApprovedPlanSnapshot? lastApproved,
        DateTimeOffset occurredAt) =>
        ApplyPlanningChange(
            assignmentId,
            progress,
            proposedPlan,
            DefaultWeekAgoHistory(assignmentId, occurredAt),
            openPending,
            lastApproved,
            occurredAt);

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
