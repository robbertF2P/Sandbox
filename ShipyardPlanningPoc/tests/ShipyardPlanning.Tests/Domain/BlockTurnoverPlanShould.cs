using ShipyardPlanning.Domain.Models;
using ShipyardPlanning.Domain.ValueObjects;
using ShipyardPlanning.Tests.Support;

namespace ShipyardPlanning.Tests.Domain;

public sealed class BlockTurnoverPlanShould
{
  [Fact]
  public void Recalculate_schedule_serializes_goliath_crane_turnovers()
  {
    BlockTurnoverPlan scheduled = FerryBerth7Scenario.DraftPlan().RecalculateSchedule();

    TurnoverOperation a12Lift = FerryBerth7Scenario.Find(scheduled, "A12-LIFT");
    TurnoverOperation b03Lift = FerryBerth7Scenario.Find(scheduled, "B03-LIFT");

    Assert.NotNull(a12Lift.ScheduledStart);
    Assert.NotNull(b03Lift.ScheduledStart);
    Assert.Equal(a12Lift.ScheduledEnd, b03Lift.ScheduledStart);
    Assert.Equal(new WorkMinutes(900), scheduled.CriticalPathLength);
  }

  [Fact]
  public void Crane_breakdown_ripples_block_b_lift_and_extends_plan_end()
  {
    BlockTurnoverPlan baseline = FerryBerth7Scenario.DraftPlan().RecalculateSchedule();
    DateTimeOffset breakdownAt = FerryBerth7Scenario.Find(baseline, "A12-LIFT").ScheduledStart!.Value;
    TimeSpan downtime = TimeSpan.FromHours(6);

    BlockTurnoverPlan disrupted = baseline.WithCraneBreakdown(FerryBerth7Scenario.Goliath, breakdownAt, downtime);

    TurnoverOperation b03Lift = FerryBerth7Scenario.Find(disrupted, "B03-LIFT");
    TurnoverOperation baselineB03Lift = FerryBerth7Scenario.Find(baseline, "B03-LIFT");

    Assert.Equal(baselineB03Lift.ScheduledStart!.Value.Add(downtime), b03Lift.ScheduledStart);
    Assert.Equal(baseline.PlanEnd!.Value.Add(downtime), disrupted.PlanEnd);
    Assert.Empty(disrupted.ValidateForCommit());
  }

  [Fact]
  public void Commit_rejects_unscheduled_operations()
  {
    BlockTurnoverPlan draft = FerryBerth7Scenario.DraftPlan();

    InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => draft.Commit());

    Assert.Contains("must be scheduled", error.Message);
  }

  [Fact]
  public void Commit_succeeds_when_schedule_is_valid()
  {
    BlockTurnoverPlan committed = FerryBerth7Scenario.DraftPlan().RecalculateSchedule().Commit();

    Assert.Equal(TurnoverPlanStatus.Committed, committed.Status);
    Assert.Empty(committed.ValidateForCommit());
  }
}
