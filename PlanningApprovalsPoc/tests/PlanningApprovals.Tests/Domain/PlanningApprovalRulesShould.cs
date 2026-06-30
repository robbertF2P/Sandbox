using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.Rules;
using PlanningApprovals.Domain.ValueObjects;
using PlanningApprovals.Tests.Support;

namespace PlanningApprovals.Tests.Domain;

public sealed class PlanningApprovalRulesShould
{
    [Fact]
    public void ResolveState_ReturnsApproved_WhenCurrentMatchesLastApproval()
    {
        ApprovalValues values = PlanningApprovalScenario.Values(
            12.5m,
            new DateOnly(2026, 6, 10),
            new DateOnly(2026, 6, 24),
            "j.doe");

        AssignmentApprovalRecord approval = AssignmentApprovalRecord.Create(
            PlanningApprovalScenario.AssignmentWelding,
            DateOnly.FromDateTime(PlanningApprovalScenario.Today.UtcDateTime),
            PlanningApprovalScenario.ForemanPersonId,
            PlanningApprovalScenario.Today,
            values);

        AssignmentApprovalState state = PlanningApprovalRules.ResolveState(values, approval);

        Assert.Equal(AssignmentApprovalState.Approved, state);
    }

    [Fact]
    public void ResolveState_ReturnsNotApproved_WhenHoursToGoChanged()
    {
        ApprovalValues approved = PlanningApprovalScenario.Values(
            12.5m,
            new DateOnly(2026, 6, 10),
            new DateOnly(2026, 6, 24),
            "j.doe");

        AssignmentApprovalRecord approval = AssignmentApprovalRecord.Create(
            PlanningApprovalScenario.AssignmentWelding,
            DateOnly.FromDateTime(PlanningApprovalScenario.Today.UtcDateTime),
            PlanningApprovalScenario.ForemanPersonId,
            PlanningApprovalScenario.Today,
            approved);

        ApprovalValues changed = approved with { HoursToGo = 15m };
        AssignmentApprovalState state = PlanningApprovalRules.ResolveState(changed, approval);

        Assert.Equal(AssignmentApprovalState.NotApproved, state);
    }

    [Fact]
    public void ResolveApprovalDay_UsesUtcDate()
    {
        var approvedAtUtc = new DateTimeOffset(2026, 6, 30, 23, 30, 0, TimeSpan.Zero);

        DateOnly approvalDay = PlanningApprovalRules.ResolveApprovalDay(approvedAtUtc);

        Assert.Equal(new DateOnly(2026, 6, 30), approvalDay);
    }
}
