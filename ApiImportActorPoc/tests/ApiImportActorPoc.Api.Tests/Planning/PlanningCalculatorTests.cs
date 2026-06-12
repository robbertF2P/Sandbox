using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Planning;
using ApiImportActorPoc.Core.Planning;

namespace ApiImportActorPoc.Api.Tests.Planning;

public sealed class PlanningCalculatorTests
{
    [Fact]
    public void Calculate_SchedulesSuccessorAfterPredecessorEnds()
    {
        var activities = new List<PlanningActivitySnapshot>
        {
            Activity(1, "Erection", "Block", 3, []),
            Activity(2, "Welding", "Block", 2, [
                new ActivityRelationModel(1, ActivityRelationType.Predecessor)
            ])
        };

        var plan = PlanningCalculator.Calculate(
            1,
            "MV Alpha",
            new DateOnly(2026, 1, 6),
            activities,
            [],
            DateTimeOffset.UtcNow);

        var erection = plan.Activities.Single(row => row.ActivityId == 1);
        var welding = plan.Activities.Single(row => row.ActivityId == 2);

        Assert.Equal(new DateOnly(2026, 1, 6), erection.StartDate);
        Assert.Equal(new DateOnly(2026, 1, 8), erection.EndDate);
        Assert.Equal(new DateOnly(2026, 1, 9), welding.StartDate);
        Assert.Equal(new DateOnly(2026, 1, 10), welding.EndDate);
        Assert.Equal(new DateOnly(2026, 1, 10), plan.PlannedEndDate);
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
            new DateOnly(2026, 2, 1),
            activities,
            [],
            DateTimeOffset.UtcNow);

        var later = PlanningCalculator.Calculate(
            1,
            "MV Beta",
            new DateOnly(2026, 3, 1),
            activities,
            [],
            DateTimeOffset.UtcNow);

        Assert.Equal(new DateOnly(2026, 2, 1), early.Activities[0].StartDate);
        Assert.Equal(new DateOnly(2026, 3, 1), later.Activities[0].StartDate);
        Assert.Equal(28, later.Activities[0].StartDate.DayNumber - early.Activities[0].StartDate.DayNumber);
    }

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
            [new PlanningAssignmentSnapshot(id * 10, "Trade", durationDays)],
            relations);
}
