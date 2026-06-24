using Microsoft.EntityFrameworkCore;
using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.Services;
using PlanningApprovals.Domain.ValueObjects;
using PlanningApprovals.Infrastructure;
using PlanningApprovals.Tests.Support;

namespace PlanningApprovals.Tests.Integration;

public sealed class PlanningApprovalsPersistenceShould
{
    private static readonly DateTimeOffset Now = new(2026, 6, 24, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Persists_request_decision_snapshot_and_audit_history()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"planning-approvals-{Guid.NewGuid():N}.db");
        PlanningApprovalsDbContextFactory factory = new($"Data Source={databasePath}");

        try
        {
            ProgressRevisionRef progress = Floor2PlanApprovalScenario.Progress(
                Floor2PlanApprovalScenario.AssignmentWelding,
                revisionId: 42,
                percentComplete: 55m,
                bookedHours: 165m,
                Now);

            PlanSnapshot plan = Floor2PlanApprovalScenario.Plan(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 23),
                200m,
                "profile-v1",
                "run-2");

            ApprovalSyncResult sync = Floor2PlanApprovalScenario.ApplyPlanningChange(
                Floor2PlanApprovalScenario.AssignmentWelding,
                progress,
                plan,
                openPending: null,
                lastApproved: null,
                Now);

            AssignmentApprovalRequest request = sync.Actions
                .Single(action => action.Kind == ApprovalSyncActionKind.OpenRequest)
                .OpenedRequest!;

            ForemanDecisionResult approved = Floor2PlanApprovalScenario.Approve(request, Now.AddHours(1));

            await using (PlanningApprovalsDbContext writeContext = factory.CreateDbContext())
            {
                writeContext.ApprovalRequests.Add(request);
                writeContext.ApprovalDecisions.Add(approved.Decision);
                if (approved.Snapshot is not null)
                {
                    writeContext.ApprovedPlanSnapshots.Add(approved.Snapshot);
                }

                await writeContext.SaveChangesAsync();
            }

            await using PlanningApprovalsDbContext readContext = factory.CreateDbContext();

            AssignmentApprovalRequest loadedRequest = await readContext.ApprovalRequests
                .SingleAsync();

            ApprovalDecision loadedDecision = await readContext.ApprovalDecisions.SingleAsync();
            ApprovedPlanSnapshot loadedSnapshot = await readContext.ApprovedPlanSnapshots.SingleAsync();

            Assert.Equal(ApprovalRequestStatus.Approved, loadedRequest.Status);
            Assert.Equal(ApprovalDecisionType.Approved, loadedDecision.Decision);
            Assert.Equal(loadedDecision.PublicId, loadedSnapshot.DecisionPublicId);
            Assert.Equal(progress.Fingerprint, loadedSnapshot.ProgressRevision.Fingerprint);
            Assert.Equal(plan.Fingerprint, loadedSnapshot.PlanSnapshot.Fingerprint);
            Assert.Equal(30m, loadedRequest.LookbackBaseline.ProgressRevision.PercentComplete);
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    [Fact]
    public async Task Reopened_request_after_new_progress_keeps_prior_decision_in_history()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"planning-approvals-{Guid.NewGuid():N}.db");
        PlanningApprovalsDbContextFactory factory = new($"Data Source={databasePath}");

        try
        {
            ProgressRevisionRef firstProgress = Floor2PlanApprovalScenario.Progress(
                Floor2PlanApprovalScenario.AssignmentWelding,
                revisionId: 1,
                percentComplete: 40m,
                bookedHours: 120m,
                Now);

            PlanSnapshot firstPlan = Floor2PlanApprovalScenario.Plan(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 20),
                200m,
                "profile-v1",
                "run-1");

            AssignmentApprovalRequest firstRequest = Floor2PlanApprovalScenario.ApplyPlanningChange(
                Floor2PlanApprovalScenario.AssignmentWelding,
                firstProgress,
                firstPlan,
                openPending: null,
                lastApproved: null,
                Now).Actions.Single(action => action.Kind == ApprovalSyncActionKind.OpenRequest).OpenedRequest!;

            ForemanDecisionResult firstApproval = Floor2PlanApprovalScenario.Approve(
                firstRequest,
                Now.AddHours(1),
                correlationId: "first-approval");

            ProgressRevisionRef secondProgress = Floor2PlanApprovalScenario.Progress(
                Floor2PlanApprovalScenario.AssignmentWelding,
                revisionId: 2,
                percentComplete: 55m,
                bookedHours: 165m,
                Now.AddDays(1));

            PlanSnapshot secondPlan = Floor2PlanApprovalScenario.Plan(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 23),
                200m,
                "profile-v1",
                "run-2");

            AssignmentApprovalRequest secondRequest = Floor2PlanApprovalScenario.ApplyPlanningChange(
                Floor2PlanApprovalScenario.AssignmentWelding,
                secondProgress,
                secondPlan,
                openPending: null,
                lastApproved: firstApproval.Snapshot,
                Now.AddDays(1)).Actions.Single(action => action.Kind == ApprovalSyncActionKind.OpenRequest).OpenedRequest!;

            await using (PlanningApprovalsDbContext writeContext = factory.CreateDbContext())
            {
                writeContext.ApprovalRequests.AddRange(firstRequest, secondRequest);
                writeContext.ApprovalDecisions.Add(firstApproval.Decision);
                writeContext.ApprovedPlanSnapshots.Add(firstApproval.Snapshot!);
                await writeContext.SaveChangesAsync();
            }

            await using PlanningApprovalsDbContext readContext = factory.CreateDbContext();

            Assert.Equal(2, await readContext.ApprovalRequests.CountAsync());
            Assert.Equal(1, await readContext.ApprovalDecisions.CountAsync());
            Assert.Equal(1, await readContext.ApprovedPlanSnapshots.CountAsync());

            AssignmentApprovalRequest pending = await readContext.ApprovalRequests
                .SingleAsync(request => request.Status == ApprovalRequestStatus.Pending);

            Assert.Equal(secondProgress.RevisionId, pending.ProgressRevision.RevisionId);
            Assert.Equal(firstApproval.Snapshot!.PublicId, pending.LastApprovedSnapshotId);
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    [Fact]
    public async Task Persists_planning_checkpoints_for_lookback_resolution()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"planning-approvals-{Guid.NewGuid():N}.db");
        PlanningApprovalsDbContextFactory factory = new($"Data Source={databasePath}");

        try
        {
            long assignmentId = Floor2PlanApprovalScenario.AssignmentWelding;
            AssignmentPlanningCheckpoint checkpoint = Floor2PlanApprovalScenario.Checkpoint(
                assignmentId,
                Now.AddDays(-8),
                Floor2PlanApprovalScenario.Progress(assignmentId, 1, 30m, 90m, Now.AddDays(-8)),
                Floor2PlanApprovalScenario.Plan(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 18), 200m, "p", "r1"));

            await using (PlanningApprovalsDbContext writeContext = factory.CreateDbContext())
            {
                writeContext.PlanningCheckpoints.Add(checkpoint);
                await writeContext.SaveChangesAsync();
            }

            await using PlanningApprovalsDbContext readContext = factory.CreateDbContext();

            AssignmentPlanningCheckpoint loaded = await readContext.PlanningCheckpoints.SingleAsync();
            PlanningStateSnapshot? baseline = LookbackBaselineResolver.Resolve(
                [loaded],
                Now,
                ApprovalLookbackWindow.OneWeek);

            Assert.NotNull(baseline);
            Assert.Equal(30m, baseline.ProgressRevision.PercentComplete);
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}
