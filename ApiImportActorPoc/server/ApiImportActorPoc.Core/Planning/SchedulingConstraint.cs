using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Core.Planning;

public enum SchedulingDependencyType
{
    FinishToStart,
    StartToStart,
    FinishToFinish,
    StartToFinish
}

public sealed record SchedulingConstraint(
    int PredecessorActivityId,
    SchedulingDependencyType DependencyType,
    LagDays LagDays);

public static class ActivityRelationScheduling
{
    public static void AddSchedulingConstraints(
        int sourceActivityId,
        ActivityRelationModel relation,
        IDictionary<int, List<SchedulingConstraint>> constraintsBySuccessor)
    {
        if (relation.Type == ActivityRelationType.Child)
        {
            return;
        }

        if (relation.Type == ActivityRelationType.Successor)
        {
            AddConstraint(
                constraintsBySuccessor,
                relation.RelatedActivityId,
                new SchedulingConstraint(sourceActivityId, SchedulingDependencyType.FinishToStart, relation.LagDays));
            return;
        }

        if (!TryMapDependencyType(relation.Type, out var dependencyType))
        {
            return;
        }

        AddConstraint(
            constraintsBySuccessor,
            sourceActivityId,
            new SchedulingConstraint(relation.RelatedActivityId, dependencyType, relation.LagDays));
    }

    public static bool TryMapDependencyType(
        ActivityRelationType relationType,
        out SchedulingDependencyType dependencyType)
    {
        dependencyType = relationType switch
        {
            ActivityRelationType.Predecessor or ActivityRelationType.FinishToStart => SchedulingDependencyType.FinishToStart,
            ActivityRelationType.StartToStart => SchedulingDependencyType.StartToStart,
            ActivityRelationType.FinishToFinish => SchedulingDependencyType.FinishToFinish,
            ActivityRelationType.StartToFinish => SchedulingDependencyType.StartToFinish,
            _ => default
        };

        return relationType is ActivityRelationType.Predecessor
            or ActivityRelationType.FinishToStart
            or ActivityRelationType.StartToStart
            or ActivityRelationType.FinishToFinish
            or ActivityRelationType.StartToFinish;
    }

    private static void AddConstraint(
        IDictionary<int, List<SchedulingConstraint>> constraintsBySuccessor,
        int successorActivityId,
        SchedulingConstraint constraint)
    {
        if (!constraintsBySuccessor.TryGetValue(successorActivityId, out var constraints))
        {
            constraints = [];
            constraintsBySuccessor[successorActivityId] = constraints;
        }

        constraints.Add(constraint);
    }
}
