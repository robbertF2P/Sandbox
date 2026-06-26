using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.Services;
using PlanningApprovals.Domain.ValueObjects;
using PlanningApprovals.Tests.Support;

namespace PlanningApprovals.Tests.Domain;

public sealed class LookbackBaselineResolverShould
{
    [Fact]
    public void Resolves_checkpoint_at_or_before_one_week_cutoff()
    {
        DateTimeOffset asOf = Floor2PlanApprovalScenario.Today;
        AssignmentId assignmentId = Floor2PlanApprovalScenario.AssignmentWelding;

        AssignmentPlanningCheckpoint tooRecent = Floor2PlanApprovalScenario.Checkpoint(
            assignmentId,
            capturedAt: asOf.AddDays(-5),
            Floor2PlanApprovalScenario.Progress(assignmentId, 9, 50m, 150m, asOf.AddDays(-5)),
            Floor2PlanApprovalScenario.Plan(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 22), 200m, "p", "r9"));

        AssignmentPlanningCheckpoint weekAgo = Floor2PlanApprovalScenario.Checkpoint(
            assignmentId,
            capturedAt: asOf.AddDays(-8),
            Floor2PlanApprovalScenario.Progress(assignmentId, 1, 30m, 90m, asOf.AddDays(-8)),
            Floor2PlanApprovalScenario.Plan(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 18), 200m, "p", "r1"));

        AssignmentPlanningCheckpoint older = Floor2PlanApprovalScenario.Checkpoint(
            assignmentId,
            capturedAt: asOf.AddDays(-14),
            Floor2PlanApprovalScenario.Progress(assignmentId, 0, 10m, 30m, asOf.AddDays(-14)),
            Floor2PlanApprovalScenario.Plan(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 15), 200m, "p", "r0"));

        PlanningStateSnapshot? baseline = LookbackBaselineResolver.Resolve(
            [tooRecent, weekAgo, older],
            asOf,
            ApprovalLookbackWindow.OneWeek);

        Assert.NotNull(baseline);
        Assert.Equal(weekAgo.CapturedAt, baseline.CapturedAt);
        Assert.Equal(30m, baseline.ProgressRevision.PercentComplete);
    }
}
