using HourApprovals.Domain.Enums;
using HourApprovals.Domain.Models;
using HourApprovals.Domain.Rules;
using HourApprovals.Domain.ValueObjects;

namespace HourApprovals.Unit.Tests;

[Trait("Module", "HourApprovals")]
public sealed class HourApprovalRulesShould
{
    private static readonly ApprovalValues Baseline = new(10m, 25m, 40m, null, null);

    [Fact]
    public void ResolveState_ReturnsApproved_WhenCurrentMatchesLastApproval()
    {
        ApprovalRecord approval = ApprovalRecord.Create(
            Guid.NewGuid(),
            "supervisor.demo",
            DateTimeOffset.UtcNow,
            Baseline);

        TaskApprovalState state = HourApprovalRules.ResolveState(Baseline, approval);

        Assert.Equal(TaskApprovalState.Approved, state);
    }

    [Fact]
    public void ResolveState_ReturnsNotApproved_WhenValuesChanged()
    {
        ApprovalRecord approval = ApprovalRecord.Create(
            Guid.NewGuid(),
            "supervisor.demo",
            DateTimeOffset.UtcNow,
            Baseline);

        var changed = Baseline with { HoursToGo = 12m };
        TaskApprovalState state = HourApprovalRules.ResolveState(changed, approval);

        Assert.Equal(TaskApprovalState.NotApproved, state);
    }

    [Fact]
    public void MatchesFilter_FiltersApprovedAndNotApproved()
    {
        Assert.True(HourApprovalRules.MatchesFilter(TaskApprovalState.Approved, ApprovalFilterStatus.Approved));
        Assert.False(HourApprovalRules.MatchesFilter(TaskApprovalState.Approved, ApprovalFilterStatus.NotApproved));
        Assert.True(HourApprovalRules.MatchesFilter(TaskApprovalState.NotApproved, ApprovalFilterStatus.All));
    }

    [Fact]
    public void CanApprove_RequiresPermission()
    {
        Assert.True(HourApprovalRules.CanApprove([HourApprovalRules.ApproveHoursProgressPermission]));
        Assert.False(HourApprovalRules.CanApprove([]));
    }
}
