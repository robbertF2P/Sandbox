using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Services;

public sealed record ApprovalSyncAction(
    ApprovalSyncActionKind Kind,
    AssignmentApprovalRequest? SupersededRequest = null,
    AssignmentApprovalRequest? OpenedRequest = null)
{
    public static ApprovalSyncAction None { get; } = new(ApprovalSyncActionKind.None);

    public static ApprovalSyncAction Supersede(AssignmentApprovalRequest request) =>
        new(ApprovalSyncActionKind.SupersedePendingRequest, request);

    public static ApprovalSyncAction Open(AssignmentApprovalRequest request) =>
        new(ApprovalSyncActionKind.OpenRequest, OpenedRequest: request);
}

public enum ApprovalSyncActionKind
{
    None = 0,
    SupersedePendingRequest = 1,
    OpenRequest = 2,
}

public sealed record ApprovalSyncResult(IReadOnlyList<ApprovalSyncAction> Actions)
{
    public static ApprovalSyncResult NoAction { get; } = new([]);

    public bool RequiresPersistence => Actions.Count > 0;
}

public sealed record ForemanDecisionResult(
    AssignmentApprovalRequest Request,
    ApprovalDecision Decision,
    ApprovedPlanSnapshot? Snapshot);

public static class PlanningApprovalCoordinator
{
    public static ApprovalSyncResult SynchronizeAfterPlanningChange(
        long projectId,
        long assignmentId,
        ProgressRevisionRef currentProgress,
        PlanSnapshot proposedPlan,
        IEnumerable<AssignmentPlanningCheckpoint> checkpointHistory,
        AssignmentApprovalRequest? openPendingRequest,
        ApprovedPlanSnapshot? lastApproved,
        DateTimeOffset occurredAt,
        string openedByProcess,
        ApprovalLookbackWindow? lookbackWindow = null)
    {
        ApprovalLookbackWindow window = lookbackWindow ?? ApprovalLookbackWindow.OneWeek;

        PlanningStateSnapshot? lookbackBaseline = LookbackBaselineResolver.Resolve(
            checkpointHistory,
            occurredAt,
            window);

        StalenessEvaluation evaluation = ApprovalStalenessEvaluator.Evaluate(
            currentProgress,
            proposedPlan,
            lookbackBaseline,
            lastApproved);

        if (!evaluation.RequiresApproval)
        {
            return ApprovalSyncResult.NoAction;
        }

        if (openPendingRequest is not null
            && openPendingRequest.Status == ApprovalRequestStatus.Pending
            && MatchesCurrentWork(openPendingRequest, currentProgress, proposedPlan))
        {
            return ApprovalSyncResult.NoAction;
        }

        PlanningStateSnapshot baseline = lookbackBaseline
            ?? throw new InvalidOperationException(
                $"No planning checkpoint found at or before the {window.Duration.TotalDays:0}-day lookback for assignment {assignmentId}.");

        List<ApprovalSyncAction> actions = [];

        if (openPendingRequest is { Status: ApprovalRequestStatus.Pending })
        {
            openPendingRequest.MarkSuperseded(occurredAt);
            actions.Add(ApprovalSyncAction.Supersede(openPendingRequest));
        }

        AssignmentApprovalRequest opened = AssignmentApprovalRequest.Open(
            projectId,
            assignmentId,
            evaluation.RequiredBecause,
            currentProgress,
            proposedPlan,
            baseline,
            lastApproved,
            occurredAt,
            openedByProcess);

        actions.Add(ApprovalSyncAction.Open(opened));
        return new ApprovalSyncResult(actions);
    }

    public static ForemanDecisionResult RecordForemanDecision(
        AssignmentApprovalRequest request,
        ApprovalDecisionType decision,
        long foremanPersonId,
        DateTimeOffset decidedAt,
        string? comment,
        string correlationId,
        Guid? batchPublicId)
    {
        ApprovalDecision recorded = ApprovalDecision.Record(
            request,
            decision,
            foremanPersonId,
            decidedAt,
            comment,
            correlationId,
            batchPublicId);

        ApprovedPlanSnapshot? snapshot = null;

        switch (decision)
        {
            case ApprovalDecisionType.Approved:
                request.MarkApproved(decidedAt);
                snapshot = ApprovedPlanSnapshot.FromApproval(recorded, request);
                break;
            case ApprovalDecisionType.Rejected:
                request.MarkRejected(decidedAt);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(decision), decision, "Unsupported decision.");
        }

        return new ForemanDecisionResult(request, recorded, snapshot);
    }

    private static bool MatchesCurrentWork(
        AssignmentApprovalRequest request,
        ProgressRevisionRef currentProgress,
        PlanSnapshot proposedPlan) =>
        string.Equals(request.ProgressRevision.Fingerprint, currentProgress.Fingerprint, StringComparison.Ordinal)
        && string.Equals(request.ProposedPlan.Fingerprint, proposedPlan.Fingerprint, StringComparison.Ordinal);
}
