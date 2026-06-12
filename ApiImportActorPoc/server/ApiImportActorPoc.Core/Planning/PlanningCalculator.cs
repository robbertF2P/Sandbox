using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Planning;
using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Core.Planning;

public static class PlanningCalculator
{
    private static readonly DurationDays MinimumDuration = DurationDays.From(0.5m);

    public static GanttProjectPlanDto Calculate(
        int projectId,
        string projectName,
        ScheduleDate plannedStartDate,
        IReadOnlyList<PlanningActivitySnapshot> activities,
        IReadOnlyList<GanttMilestoneDto> milestones,
        DateTimeOffset calculatedAt)
    {
        if (activities.Count == 0)
        {
            return new GanttProjectPlanDto(
                projectId,
                projectName,
                plannedStartDate,
                plannedStartDate,
                calculatedAt,
                [],
                milestones);
        }

        var activityEnds = ScheduleActivities(plannedStartDate, activities);
        var rows = activities
            .Select(activity => ToActivityRow(activity, activityEnds[activity.Id]))
            .OrderBy(row => row.StartDate.Value)
            .ThenBy(row => row.ActivityName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var plannedEnd = rows.Max(row => row.EndDate);

        return new GanttProjectPlanDto(
            projectId,
            projectName,
            plannedStartDate,
            plannedEnd,
            calculatedAt,
            rows,
            milestones);
    }

    public static DurationDays ResolveDurationDays(DurationDays? plannedDuration, Hours budgetedHours) =>
        plannedDuration is { Value: > 0 }
            ? plannedDuration.Value
            : DurationDays.FromBudgetedHours(budgetedHours);

    private static Dictionary<int, (ScheduleDate Start, ScheduleDate End)> ScheduleActivities(
        ScheduleDate projectStart,
        IReadOnlyList<PlanningActivitySnapshot> activities)
    {
        var durations = activities.ToDictionary(
            activity => activity.Id,
            activity => activity.Assignments.Count == 0
                ? MinimumDuration
                : activity.Assignments.Max(assignment => assignment.DurationDays));

        var constraintsBySuccessor = BuildConstraints(activities);
        var predecessors = BuildPredecessorMap(constraintsBySuccessor);
        var schedule = new Dictionary<int, (ScheduleDate Start, ScheduleDate End)>();
        var ordered = TopologicalOrder(activities, predecessors);

        foreach (var activityId in ordered)
        {
            var duration = durations[activityId];
            var earliestStart = projectStart;

            if (constraintsBySuccessor.TryGetValue(activityId, out var constraints))
            {
                foreach (var constraint in constraints)
                {
                    var (predecessorStart, predecessorEnd) = schedule[constraint.PredecessorActivityId];
                    var requiredStart = RequiredStartDate(
                        constraint.DependencyType,
                        predecessorStart,
                        predecessorEnd,
                        duration,
                        constraint.LagDays);

                    earliestStart = constraint.DependencyType switch
                    {
                        SchedulingDependencyType.StartToFinish when requiredStart < earliestStart => requiredStart,
                        SchedulingDependencyType.StartToFinish => earliestStart,
                        _ when requiredStart > earliestStart => requiredStart,
                        _ => earliestStart
                    };
                }
            }

            var end = AddWorkingDuration(earliestStart, duration);
            schedule[activityId] = (earliestStart, end);
        }

        return schedule;
    }

    private static Dictionary<int, List<SchedulingConstraint>> BuildConstraints(
        IReadOnlyList<PlanningActivitySnapshot> activities)
    {
        var constraintsBySuccessor = activities.ToDictionary(activity => activity.Id, _ => new List<SchedulingConstraint>());

        foreach (var activity in activities)
        {
            foreach (var relation in activity.Relations)
            {
                ActivityRelationScheduling.AddSchedulingConstraints(
                    activity.Id,
                    relation,
                    constraintsBySuccessor);
            }
        }

        return constraintsBySuccessor;
    }

    private static Dictionary<int, HashSet<int>> BuildPredecessorMap(
        IReadOnlyDictionary<int, List<SchedulingConstraint>> constraintsBySuccessor)
    {
        var predecessors = constraintsBySuccessor.Keys.ToDictionary(id => id, _ => new HashSet<int>());

        foreach (var (successorId, constraints) in constraintsBySuccessor)
        {
            foreach (var constraint in constraints)
            {
                predecessors[successorId].Add(constraint.PredecessorActivityId);
            }
        }

        return predecessors;
    }

    public static ScheduleDate RequiredStartDate(
        SchedulingDependencyType dependencyType,
        ScheduleDate predecessorStart,
        ScheduleDate predecessorEnd,
        DurationDays successorDuration,
        LagDays lagDays)
    {
        var spanDays = (int)Math.Max(1, Math.Ceiling(successorDuration.Value));

        return dependencyType switch
        {
            SchedulingDependencyType.FinishToStart => predecessorEnd.AddDays(1 + lagDays.Value),
            SchedulingDependencyType.StartToStart => predecessorStart.AddDays(lagDays.Value),
            SchedulingDependencyType.FinishToFinish => predecessorEnd.AddDays(lagDays.Value).AddDays(-(spanDays - 1)),
            SchedulingDependencyType.StartToFinish => predecessorStart.AddDays(lagDays.Value).AddDays(-(spanDays - 1)),
            _ => predecessorEnd.AddDays(1 + lagDays.Value)
        };
    }

    private static List<int> TopologicalOrder(
        IReadOnlyList<PlanningActivitySnapshot> activities,
        IReadOnlyDictionary<int, HashSet<int>> predecessors)
    {
        var ids = activities.Select(activity => activity.Id).ToHashSet();
        var incoming = predecessors.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Where(ids.Contains).Count());

        var queue = new Queue<int>(incoming.Where(pair => pair.Value == 0).Select(pair => pair.Key).OrderBy(id => id));
        var ordered = new List<int>();

        var successors = new Dictionary<int, List<int>>();
        foreach (var (activityId, preds) in predecessors)
        {
            foreach (var predecessorId in preds.Where(ids.Contains))
            {
                if (!successors.TryGetValue(predecessorId, out var list))
                {
                    list = [];
                    successors[predecessorId] = list;
                }

                list.Add(activityId);
            }
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            ordered.Add(current);

            if (!successors.TryGetValue(current, out var nextActivities))
            {
                continue;
            }

            foreach (var next in nextActivities.OrderBy(id => id))
            {
                incoming[next]--;
                if (incoming[next] == 0)
                {
                    queue.Enqueue(next);
                }
            }
        }

        if (ordered.Count != activities.Count)
        {
            ordered.AddRange(activities.Select(activity => activity.Id).Except(ordered).OrderBy(id => id));
        }

        return ordered;
    }

    private static GanttActivityRowDto ToActivityRow(
        PlanningActivitySnapshot activity,
        (ScheduleDate Start, ScheduleDate End) schedule)
    {
        var assignmentRows = activity.Assignments
            .Select(assignment => new GanttAssignmentRowDto(
                assignment.Id,
                assignment.Label,
                assignment.DurationDays,
                schedule.Start,
                AddWorkingDuration(schedule.Start, assignment.DurationDays)))
            .ToList();

        return new GanttActivityRowDto(
            activity.Id,
            activity.Name,
            activity.ComponentName,
            schedule.Start,
            schedule.End,
            assignmentRows);
    }

    private static ScheduleDate AddWorkingDuration(ScheduleDate start, DurationDays durationDays)
    {
        var spanDays = (int)Math.Max(1, Math.Ceiling(durationDays.Value));
        return start.AddDays(spanDays - 1);
    }
}
