using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Values;
using ApiImportActorPoc.Core.Planning;

namespace ApiImportActorPoc.Api.Tests.Planning;

public sealed class ActivityRelationSchedulingTests
{
    [Fact]
    public void AddSchedulingConstraints_MapsSuccessorToFinishToStartOnTargetActivity()
    {
        var constraints = new Dictionary<int, List<SchedulingConstraint>> { [10] = [], [20] = [] };

        ActivityRelationScheduling.AddSchedulingConstraints(
            10,
            new ActivityRelationModel(20, ActivityRelationType.Successor),
            constraints);

        var constraint = Assert.Single(constraints[20]);
        Assert.Equal(10, constraint.PredecessorActivityId);
        Assert.Equal(SchedulingDependencyType.FinishToStart, constraint.DependencyType);
    }

    [Fact]
    public void AddSchedulingConstraints_MapsStartToFinishOnSourceActivity()
    {
        var constraints = new Dictionary<int, List<SchedulingConstraint>> { [1] = [], [2] = [] };

        ActivityRelationScheduling.AddSchedulingConstraints(
            2,
            new ActivityRelationModel(1, ActivityRelationType.StartToFinish),
            constraints);

        var constraint = Assert.Single(constraints[2]);
        Assert.Equal(1, constraint.PredecessorActivityId);
        Assert.Equal(SchedulingDependencyType.StartToFinish, constraint.DependencyType);
    }

    [Fact]
    public void RequiredStartDate_StartToFinishBackSchedulesFromPredecessorStart()
    {
        var requiredStart = PlanningCalculator.RequiredStartDate(
            SchedulingDependencyType.StartToFinish,
            ScheduleDate.From(new DateOnly(2026, 1, 6)),
            ScheduleDate.From(new DateOnly(2026, 1, 6)),
            DurationDays.From(3m),
            LagDays.Zero);

        Assert.Equal(new DateOnly(2026, 1, 4), requiredStart.Value);
    }

    [Fact]
    public void Calculate_StartToFinishAlignsSuccessorFinishToPredecessorStart()
    {
        var activities = new List<PlanningActivitySnapshot>
        {
            new(1, "Handover", "Block", [new PlanningAssignmentSnapshot(10, "T", DurationDays.From(1m))], []),
            new(2, "Shutdown", "Block", [new PlanningAssignmentSnapshot(20, "T", DurationDays.From(3m))], [
                new ActivityRelationModel(1, ActivityRelationType.StartToFinish)
            ])
        };

        var plan = PlanningCalculator.Calculate(
            1,
            "MV",
            ScheduleDate.From(new DateOnly(2026, 1, 6)),
            activities,
            [],
            DateTimeOffset.UtcNow);

        var shutdown = plan.Activities.Single(row => row.ActivityId == 2);
        Assert.Equal(new DateOnly(2026, 1, 4), shutdown.StartDate.Value);
        Assert.Equal(new DateOnly(2026, 1, 6), shutdown.EndDate.Value);
    }

    [Fact]
    public void TryMapDependencyType_SupportsAllSchedulingKinds()
    {
        Assert.True(ActivityRelationScheduling.TryMapDependencyType(ActivityRelationType.StartToStart, out var ss));
        Assert.Equal(SchedulingDependencyType.StartToStart, ss);

        Assert.True(ActivityRelationScheduling.TryMapDependencyType(ActivityRelationType.FinishToFinish, out var ff));
        Assert.Equal(SchedulingDependencyType.FinishToFinish, ff);

        Assert.True(ActivityRelationScheduling.TryMapDependencyType(ActivityRelationType.StartToFinish, out var sf));
        Assert.Equal(SchedulingDependencyType.StartToFinish, sf);

        Assert.False(ActivityRelationScheduling.TryMapDependencyType(ActivityRelationType.Child, out _));
    }
}
