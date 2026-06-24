using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.Services;
using PlanningApprovals.Domain.ValueObjects;
using PlanningApprovals.Tests.Support;

namespace PlanningApprovals.Tests.Domain;

public sealed class PlanningApprovalCoordinatorShould
{
    private static readonly DateTimeOffset Day1 = new(2026, 6, 24, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Day2 = Day1.AddDays(1);

    [Fact]
    public void Opens_request_when_progress_and_plan_change_after_approval()
    {
        ProgressRevisionRef firstProgress = Floor2PlanApprovalScenario.Progress(
            Floor2PlanApprovalScenario.AssignmentWelding,
            revisionId: 1,
            percentComplete: 40m,
            bookedHours: 120m,
            Day1);

        PlanSnapshot firstPlan = Floor2PlanApprovalScenario.Plan(
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 20),
            200m,
            "profile-v1",
            "run-1");

        ApprovalSyncResult firstSync = Floor2PlanApprovalScenario.ApplyPlanningChange(
            Floor2PlanApprovalScenario.AssignmentWelding,
            firstProgress,
            firstPlan,
            openPending: null,
            lastApproved: null,
            Day1);

        AssignmentApprovalRequest pending = firstSync.Actions
            .Single(action => action.Kind == ApprovalSyncActionKind.OpenRequest)
            .OpenedRequest!;

        ForemanDecisionResult approved = Floor2PlanApprovalScenario.Approve(pending, Day1.AddHours(2));
        ApprovedPlanSnapshot snapshot = approved.Snapshot!;

        ProgressRevisionRef secondProgress = Floor2PlanApprovalScenario.Progress(
            Floor2PlanApprovalScenario.AssignmentWelding,
            revisionId: 2,
            percentComplete: 55m,
            bookedHours: 165m,
            Day2);

        PlanSnapshot secondPlan = Floor2PlanApprovalScenario.Plan(
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 23),
            200m,
            "profile-v1",
            "run-2");

        ApprovalSyncResult secondSync = Floor2PlanApprovalScenario.ApplyPlanningChange(
            Floor2PlanApprovalScenario.AssignmentWelding,
            secondProgress,
            secondPlan,
            openPending: null,
            lastApproved: snapshot,
            Day2);

        AssignmentApprovalRequest reopened = secondSync.Actions
            .Single(action => action.Kind == ApprovalSyncActionKind.OpenRequest)
            .OpenedRequest!;

        Assert.Equal(ApprovalRequestStatus.Pending, reopened.Status);
        Assert.Equal(ApprovalRequiredBecause.Both, reopened.RequiredBecause);
        Assert.Equal(snapshot.PublicId, reopened.LastApprovedSnapshotId);
    }

    [Fact]
    public void Supersedes_open_pending_request_when_new_progress_arrives()
    {
        ProgressRevisionRef progressV1 = Floor2PlanApprovalScenario.Progress(
            Floor2PlanApprovalScenario.AssignmentFitting,
            revisionId: 10,
            percentComplete: 20m,
            bookedHours: 40m,
            Day1);

        PlanSnapshot planV1 = Floor2PlanApprovalScenario.Plan(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 15),
            80m,
            "profile-fit",
            "run-a");

        ApprovalSyncResult firstSync = Floor2PlanApprovalScenario.ApplyPlanningChange(
            Floor2PlanApprovalScenario.AssignmentFitting,
            progressV1,
            planV1,
            openPending: null,
            lastApproved: null,
            Day1);

        AssignmentApprovalRequest openPending = firstSync.Actions
            .Single(action => action.Kind == ApprovalSyncActionKind.OpenRequest)
            .OpenedRequest!;

        ProgressRevisionRef progressV2 = Floor2PlanApprovalScenario.Progress(
            Floor2PlanApprovalScenario.AssignmentFitting,
            revisionId: 11,
            percentComplete: 25m,
            bookedHours: 50m,
            Day1.AddHours(4));

        PlanSnapshot planV2 = Floor2PlanApprovalScenario.Plan(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 16),
            80m,
            "profile-fit",
            "run-b");

        ApprovalSyncResult secondSync = Floor2PlanApprovalScenario.ApplyPlanningChange(
            Floor2PlanApprovalScenario.AssignmentFitting,
            progressV2,
            planV2,
            openPending,
            lastApproved: null,
            Day1.AddHours(4));

        AssignmentApprovalRequest superseded = secondSync.Actions
            .Single(action => action.Kind == ApprovalSyncActionKind.SupersedePendingRequest)
            .SupersededRequest!;

        AssignmentApprovalRequest replacement = secondSync.Actions
            .Single(action => action.Kind == ApprovalSyncActionKind.OpenRequest)
            .OpenedRequest!;

        Assert.Equal(ApprovalRequestStatus.Superseded, superseded.Status);
        Assert.Equal(ApprovalRequestStatus.Pending, replacement.Status);
        Assert.Equal(progressV2.RevisionId, replacement.ProgressRevision.RevisionId);
    }

    [Fact]
    public void Foreman_batch_can_reference_many_pending_requests()
    {
        DateTimeOffset openedAt = Day1;
        ForemanApprovalBatch batch = ForemanApprovalBatch.Open(
            Floor2PlanApprovalScenario.ProjectId,
            Floor2PlanApprovalScenario.ForemanPersonId,
            scopeDescription: "Block A12 — week 26",
            openedAt);

        List<Guid> requestIds = [];

        for (long assignmentId = 1; assignmentId <= 250; assignmentId++)
        {
            ProgressRevisionRef progress = Floor2PlanApprovalScenario.Progress(
                assignmentId,
                revisionId: assignmentId,
                percentComplete: 10m,
                bookedHours: 8m,
                openedAt);

            PlanSnapshot plan = Floor2PlanApprovalScenario.Plan(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 30),
                40m,
                $"profile-{assignmentId}",
                $"run-{assignmentId}");

            ApprovalSyncResult sync = Floor2PlanApprovalScenario.ApplyPlanningChange(
                assignmentId,
                progress,
                plan,
                openPending: null,
                lastApproved: null,
                openedAt);

            Guid requestId = sync.Actions
                .Single(action => action.Kind == ApprovalSyncActionKind.OpenRequest)
                .OpenedRequest!.PublicId;

            requestIds.Add(requestId);
        }

        batch.AddRequests(requestIds);
        batch.Submit(openedAt.AddHours(1));

        Assert.Equal(250, batch.RequestPublicIds.Count);
        Assert.Equal(ForemanApprovalBatchStatus.Submitted, batch.Status);
    }
}
