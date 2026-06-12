using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Planning;
using ApiImportActorPoc.Contracts.Values;
using ApiImportActorPoc.Core.Planning;

namespace ApiImportActorPoc.Api.Tests.Planning;

public sealed class PlanningCalculatorTests
{
    [Fact]
    public void Calculate_SchedulesFinishToStartAfterPredecessorEnds()
    {
        var activities = new List<PlanningActivitySnapshot>
        {
            Activity(1, "Erection", "Block", 3, []),
            Activity(2, "Welding", "Block", 2, [
                new ActivityRelationModel(1, ActivityRelationType.FinishToStart)
            ])
        };

        var plan = Calculate(activities);

        var erection = plan.Activities.Single(row => row.ActivityId == 1);
        var welding = plan.Activities.Single(row => row.ActivityId == 2);

        Assert.Equal(new DateOnly(2026, 1, 6), erection.StartDate.Value);
        Assert.Equal(new DateOnly(2026, 1, 8), erection.EndDate.Value);
        Assert.Equal(new DateOnly(2026, 1, 9), welding.StartDate.Value);
        Assert.Equal(new DateOnly(2026, 1, 10), welding.EndDate.Value);
    }

    [Fact]
    public void Calculate_SchedulesStartToStartWithLag()
    {
        var activities = new List<PlanningActivitySnapshot>
        {
            Activity(1, "Prep", "Block", 5, []),
            Activity(2, "Parallel support", "Block", 2, [
                new ActivityRelationModel(1, ActivityRelationType.StartToStart, LagDays.From(2))
            ])
        };

        var plan = Calculate(activities);
        var support = plan.Activities.Single(row => row.ActivityId == 2);

        Assert.Equal(new DateOnly(2026, 1, 8), support.StartDate.Value);
        Assert.Equal(new DateOnly(2026, 1, 9), support.EndDate.Value);
    }

    [Fact]
    public void Calculate_SchedulesFinishToFinishAlignment()
    {
        var activities = new List<PlanningActivitySnapshot>
        {
            Activity(1, "Long task", "Block", 5, []),
            Activity(2, "Short closeout", "Block", 2, [
                new ActivityRelationModel(1, ActivityRelationType.FinishToFinish)
            ])
        };

        var plan = Calculate(activities);
        var longTask = plan.Activities.Single(row => row.ActivityId == 1);
        var closeout = plan.Activities.Single(row => row.ActivityId == 2);

        Assert.Equal(new DateOnly(2026, 1, 10), longTask.EndDate.Value);
        Assert.Equal(new DateOnly(2026, 1, 10), closeout.EndDate.Value);
        Assert.Equal(new DateOnly(2026, 1, 9), closeout.StartDate.Value);
    }

    [Fact]
    public void Calculate_SchedulesStartToFinishAlignment()
    {
        var activities = new List<PlanningActivitySnapshot>
        {
            Activity(1, "Handover trigger", "Block", 1, []),
            Activity(2, "Shutdown window", "Block", 3, [
                new ActivityRelationModel(1, ActivityRelationType.StartToFinish)
            ])
        };

        var plan = Calculate(activities);
        var shutdown = plan.Activities.Single(row => row.ActivityId == 2);

        Assert.Equal(new DateOnly(2026, 1, 6), shutdown.EndDate.Value);
        Assert.Equal(new DateOnly(2026, 1, 4), shutdown.StartDate.Value);
    }

    [Fact]
    public void Calculate_SupportsLegacyPredecessorAndSuccessorAsFinishToStart()
    {
        var activities = new List<PlanningActivitySnapshot>
        {
            Activity(1, "First", "Block", 2, [
                new ActivityRelationModel(2, ActivityRelationType.Successor)
            ]),
            Activity(2, "Second", "Block", 2, [])
        };

        var plan = Calculate(activities);
        var second = plan.Activities.Single(row => row.ActivityId == 2);

        Assert.Equal(new DateOnly(2026, 1, 8), second.StartDate.Value);
    }

    [Fact]
    public void Calculate_ShiftsTimelineWhenProjectStartChanges()
    {
        var activities = new List<PlanningActivitySnapshot>
        {
            Activity(1, "Trials", "Outfitting", 5, [])
        };

        var early = PlanningCalculator.Calculate(
            1,
            "MV Beta",
            ScheduleDate.From(new DateOnly(2026, 2, 1)),
            activities,
            [],
            DateTimeOffset.UtcNow);

        var later = PlanningCalculator.Calculate(
            1,
            "MV Beta",
            ScheduleDate.From(new DateOnly(2026, 3, 1)),
            activities,
            [],
            DateTimeOffset.UtcNow);

        Assert.Equal(28, later.Activities[0].StartDate.Value.DayNumber - early.Activities[0].StartDate.Value.DayNumber);
    }

    private static GanttProjectPlanDto Calculate(IReadOnlyList<PlanningActivitySnapshot> activities) =>
        PlanningCalculator.Calculate(
            1,
            "MV Alpha",
            ScheduleDate.From(new DateOnly(2026, 1, 6)),
            activities,
            [],
            DateTimeOffset.UtcNow);

    private static PlanningActivitySnapshot Activity(
        int id,
        string name,
        string component,
        decimal durationDays,
        IReadOnlyList<ActivityRelationModel> relations) =>
        new(
            id,
            name,
            component,
            [new PlanningAssignmentSnapshot(id * 10, "Trade", DurationDays.From(durationDays))],
            relations);
}
