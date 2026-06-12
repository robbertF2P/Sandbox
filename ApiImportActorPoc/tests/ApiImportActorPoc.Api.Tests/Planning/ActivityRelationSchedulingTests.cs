using ApiImportActorPoc.Contracts.Models;
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
    public void TryMapDependencyType_SupportsAllSchedulingKinds()
    {
        Assert.True(ActivityRelationScheduling.TryMapDependencyType(ActivityRelationType.StartToStart, out var ss));
        Assert.Equal(SchedulingDependencyType.StartToStart, ss);

        Assert.True(ActivityRelationScheduling.TryMapDependencyType(ActivityRelationType.FinishToFinish, out var ff));
        Assert.Equal(SchedulingDependencyType.FinishToFinish, ff);

        Assert.False(ActivityRelationScheduling.TryMapDependencyType(ActivityRelationType.Child, out _));
    }
}
