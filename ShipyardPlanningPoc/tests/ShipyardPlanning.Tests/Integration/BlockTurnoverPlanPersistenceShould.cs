using System.Collections.Immutable;
using ShipyardPlanning.Domain.Models;
using ShipyardPlanning.Domain.ValueObjects;
using ShipyardPlanning.Infrastructure;
using ShipyardPlanning.Tests.Support;

namespace ShipyardPlanning.Tests.Integration;

public sealed class BlockTurnoverPlanPersistenceShould
{
  [Fact]
  public async Task Insert_reload_and_update_immutable_graph()
  {
    string databasePath = Path.Combine(Path.GetTempPath(), $"shipyard-plan-{Guid.NewGuid():N}.db");
    PlanningDbContextFactory factory = new($"Data Source={databasePath}");

    try
    {
      BlockTurnoverPlan seeded = FerryBerth7Scenario.DraftPlan().RecalculateSchedule();
      Guid publicId;

      await using (PlanningDbContext writeContext = factory.CreateDbContext())
      {
        await writeContext.TurnoverPlans.AddImmutableAsync(seeded);
        await writeContext.SaveChangesAsync();
        publicId = seeded.PublicId;
      }

      await using (PlanningDbContext readContext = factory.CreateDbContext())
      {
        BlockTurnoverPlan loaded = await readContext.TurnoverPlans.FindImmutableAsync(publicId)
            ?? throw new InvalidOperationException("Plan was not persisted.");

        Assert.Equal("Berth 7", loaded.BerthName);
        Assert.Equal(7, loaded.Operations.Count);
      }

      await using (PlanningDbContext updateContext = factory.CreateDbContext())
      {
        BlockTurnoverPlan loaded = await updateContext.TurnoverPlans.FindImmutableAsync(publicId)
            ?? throw new InvalidOperationException("Plan was not found for update.");

        TurnoverOperation added = new(
            "C14-FIT",
            new BlockCode("C14"),
            TurnoverOperationKind.FitUp,
            new WorkMinutes(300),
            ["B03-LIFT"]);

        BlockTurnoverPlan expanded = loaded
            .WithOperations(loaded.Operations.Add(added))
            .RecalculateSchedule();

        updateContext.TurnoverPlans.UpdateImmutable(expanded);
        await updateContext.SaveChangesAsync();
      }

      await using (PlanningDbContext verifyContext = factory.CreateDbContext())
      {
        BlockTurnoverPlan reloaded = await verifyContext.TurnoverPlans.FindImmutableAsync(publicId)
            ?? throw new InvalidOperationException("Plan was not found after update.");

        Assert.Equal(8, reloaded.Operations.Count);
        Assert.NotNull(FerryBerth7Scenario.Find(reloaded, "C14-FIT").ScheduledStart);
      }
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
  public async Task Crane_breakdown_persists_after_update_immutable()
  {
    string databasePath = Path.Combine(Path.GetTempPath(), $"shipyard-plan-{Guid.NewGuid():N}.db");
    PlanningDbContextFactory factory = new($"Data Source={databasePath}");

    try
    {
      BlockTurnoverPlan baseline = FerryBerth7Scenario.DraftPlan().RecalculateSchedule();
      Guid publicId;

      await using (PlanningDbContext context = factory.CreateDbContext())
      {
        await context.TurnoverPlans.AddImmutableAsync(baseline);
        await context.SaveChangesAsync();
        publicId = baseline.PublicId;
      }

      DateTimeOffset breakdownAt;
      DateTimeOffset originalB03LiftStart;

      await using (PlanningDbContext context = factory.CreateDbContext())
      {
        BlockTurnoverPlan loaded = await context.TurnoverPlans.FindImmutableAsync(publicId)
            ?? throw new InvalidOperationException("Plan was not found.");

        originalB03LiftStart = FerryBerth7Scenario.Find(loaded, "B03-LIFT").ScheduledStart!.Value;
        breakdownAt = FerryBerth7Scenario.Find(loaded, "A12-LIFT").ScheduledStart!.Value;

        BlockTurnoverPlan disrupted = loaded
            .WithCraneBreakdown(FerryBerth7Scenario.Goliath, breakdownAt, TimeSpan.FromHours(6))
            .WithStatus(TurnoverPlanStatus.Committed);

        context.TurnoverPlans.UpdateImmutable(disrupted);
        await context.SaveChangesAsync();
      }

      await using (PlanningDbContext context = factory.CreateDbContext())
      {
        BlockTurnoverPlan reloaded = await context.TurnoverPlans.FindImmutableAsync(publicId)
            ?? throw new InvalidOperationException("Plan was not found after disruption.");

        Assert.Equal(TurnoverPlanStatus.Committed, reloaded.Status);
        Assert.Equal(
            originalB03LiftStart.AddHours(6),
            FerryBerth7Scenario.Find(reloaded, "B03-LIFT").ScheduledStart);
      }
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
