using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.Services;
using PlanningApprovals.Domain.ValueObjects;
using PlanningApprovals.Tests.Support;

namespace PlanningApprovals.Tests.Domain;

public sealed class ApprovalStalenessEvaluatorShould
{
    private static readonly DateTimeOffset Now = Floor2PlanApprovalScenario.Today;

    private static readonly PlanningStateSnapshot WeekAgoBaseline = Floor2PlanApprovalScenario.State(
        Floor2PlanApprovalScenario.WeekAgo,
        Floor2PlanApprovalScenario.Progress(
            Floor2PlanApprovalScenario.AssignmentWelding,
            revisionId: 1,
            percentComplete: 30m,
            bookedHours: 90m,
            Floor2PlanApprovalScenario.WeekAgo),
        Floor2PlanApprovalScenario.Plan(
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 18),
            200m,
            "profile-v1",
            "run-baseline"));

    [Fact]
    public void Requires_approval_when_no_lookback_baseline_exists()
    {
        ProgressRevisionRef progress = Floor2PlanApprovalScenario.Progress(
            Floor2PlanApprovalScenario.AssignmentWelding,
            revisionId: 5,
            percentComplete: 40m,
            bookedHours: 120m,
            Now);

        PlanSnapshot plan = Floor2PlanApprovalScenario.Plan(
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 20),
            200m,
            "profile-v1",
            "run-1");

        StalenessEvaluation result = ApprovalStalenessEvaluator.Evaluate(
            progress,
            plan,
            lookbackBaseline: null,
            lastApproved: null);

        Assert.True(result.RequiresApproval);
        Assert.Equal(ApprovalRequiredBecause.Both, result.RequiredBecause);
    }

    [Fact]
    public void Stays_up_to_date_when_current_matches_last_approval_even_if_lookback_differs()
    {
        ProgressRevisionRef progress = Floor2PlanApprovalScenario.Progress(
            Floor2PlanApprovalScenario.AssignmentWelding,
            revisionId: 2,
            percentComplete: 55m,
            bookedHours: 165m,
            Now);

        PlanSnapshot plan = Floor2PlanApprovalScenario.Plan(
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 23),
            200m,
            "profile-v1",
            "run-2");

        ApprovedPlanSnapshot lastApproved = new(
            Guid.NewGuid(),
            decisionPublicId: Guid.NewGuid(),
            Floor2PlanApprovalScenario.AssignmentWelding,
            Floor2PlanApprovalScenario.ForemanPersonId,
            Now.AddDays(-1),
            progress,
            plan);

        StalenessEvaluation result = ApprovalStalenessEvaluator.Evaluate(
            progress,
            plan,
            WeekAgoBaseline,
            lastApproved);

        Assert.False(result.RequiresApproval);
    }

    [Fact]
    public void Stays_up_to_date_when_unchanged_since_week_ago_baseline()
    {
        StalenessEvaluation result = ApprovalStalenessEvaluator.Evaluate(
            WeekAgoBaseline.ProgressRevision,
            WeekAgoBaseline.PlanSnapshot,
            WeekAgoBaseline,
            lastApproved: null);

        Assert.False(result.RequiresApproval);
    }

    [Fact]
    public void Requires_approval_when_only_progress_changed_since_lookback()
    {
        PlanSnapshot plan = WeekAgoBaseline.PlanSnapshot;

        ProgressRevisionRef currentProgress = Floor2PlanApprovalScenario.Progress(
            Floor2PlanApprovalScenario.AssignmentWelding,
            revisionId: 3,
            percentComplete: 60m,
            bookedHours: 180m,
            Now);

        StalenessEvaluation result = ApprovalStalenessEvaluator.Evaluate(
            currentProgress,
            plan,
            WeekAgoBaseline,
            lastApproved: null);

        Assert.True(result.RequiresApproval);
        Assert.Equal(ApprovalRequiredBecause.ProgressChanged, result.RequiredBecause);
    }
}
