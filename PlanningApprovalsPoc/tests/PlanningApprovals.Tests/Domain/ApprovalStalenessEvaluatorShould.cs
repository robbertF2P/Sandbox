using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.Services;
using PlanningApprovals.Domain.ValueObjects;
using PlanningApprovals.Tests.Support;

namespace PlanningApprovals.Tests.Domain;

public sealed class ApprovalStalenessEvaluatorShould
{
    private static readonly DateTimeOffset Now = new(2026, 6, 24, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Requires_approval_when_no_prior_snapshot_exists()
    {
        ProgressRevisionRef progress = Floor2PlanApprovalScenario.Progress(
            Floor2PlanApprovalScenario.AssignmentWelding,
            revisionId: 1,
            percentComplete: 40m,
            bookedHours: 120m,
            Now);

        PlanSnapshot plan = Floor2PlanApprovalScenario.Plan(
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 20),
            200m,
            profileFingerprint: "profile-v1",
            calculationRunId: "run-1");

        StalenessEvaluation result = ApprovalStalenessEvaluator.Evaluate(progress, plan, lastApproved: null);

        Assert.True(result.RequiresApproval);
        Assert.Equal(ApprovalRequiredBecause.Both, result.RequiredBecause);
    }

    [Fact]
    public void Stays_up_to_date_when_progress_and_plan_match_last_approval()
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
            profileFingerprint: "profile-v1",
            calculationRunId: "run-2");

        ApprovedPlanSnapshot lastApproved = new(
            Guid.NewGuid(),
            decisionPublicId: Guid.NewGuid(),
            Floor2PlanApprovalScenario.AssignmentWelding,
            Floor2PlanApprovalScenario.ForemanPersonId,
            Now.AddDays(-1),
            progress,
            plan);

        StalenessEvaluation result = ApprovalStalenessEvaluator.Evaluate(progress, plan, lastApproved);

        Assert.False(result.RequiresApproval);
    }

    [Fact]
    public void Requires_approval_when_only_progress_changed()
    {
        PlanSnapshot plan = Floor2PlanApprovalScenario.Plan(
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 23),
            200m,
            profileFingerprint: "profile-v1",
            calculationRunId: "run-2");

        ProgressRevisionRef approvedProgress = Floor2PlanApprovalScenario.Progress(
            Floor2PlanApprovalScenario.AssignmentWelding,
            revisionId: 2,
            percentComplete: 55m,
            bookedHours: 165m,
            Now.AddDays(-1));

        ProgressRevisionRef currentProgress = Floor2PlanApprovalScenario.Progress(
            Floor2PlanApprovalScenario.AssignmentWelding,
            revisionId: 3,
            percentComplete: 60m,
            bookedHours: 180m,
            Now);

        ApprovedPlanSnapshot lastApproved = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Floor2PlanApprovalScenario.AssignmentWelding,
            Floor2PlanApprovalScenario.ForemanPersonId,
            Now.AddDays(-1),
            approvedProgress,
            plan);

        StalenessEvaluation result = ApprovalStalenessEvaluator.Evaluate(currentProgress, plan, lastApproved);

        Assert.True(result.RequiresApproval);
        Assert.Equal(ApprovalRequiredBecause.ProgressChanged, result.RequiredBecause);
    }
}
