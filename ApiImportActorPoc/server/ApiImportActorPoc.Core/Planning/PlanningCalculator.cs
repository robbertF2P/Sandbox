using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Planning;

namespace ApiImportActorPoc.Core.Planning;

public static class PlanningCalculator
{
    private const decimal DefaultHoursPerDay = 8m;
    private const decimal MinimumDurationDays = 0.5m;

    public static GanttProjectPlanDto Calculate(
        int projectId,
        string projectName,
        DateOnly plannedStartDate,
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
            .OrderBy(row => row.StartDate)
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

    public static decimal ResolveDurationDays(decimal? plannedDurationDays, decimal budgetedHours) =>
        plannedDurationDays is > 0
            ? plannedDurationDays.Value
            : Math.Max(MinimumDurationDays, Math.Round(budgetedHours / DefaultHoursPerDay, 2));

    private static Dictionary<int, (DateOnly Start, DateOnly End)> ScheduleActivities(
        DateOnly projectStart,
        IReadOnlyList<PlanningActivitySnapshot> activities)
    {
        var durations = activities.ToDictionary(
            activity => activity.Id,
            activity => Math.Max(
                MinimumDurationDays,
                activity.Assignments.Count == 0
                    ? MinimumDurationDays
                    : activity.Assignments.Max(assignment => assignment.DurationDays)));

        var predecessors = BuildPredecessorMap(activities);
        var schedule = new Dictionary<int, (DateOnly Start, DateOnly End)>();
        var ordered = TopologicalOrder(activities, predecessors);

        foreach (var activityId in ordered)
        {
            var start = projectStart;
            if (predecessors.TryGetValue(activityId, out var requiredPredecessors) && requiredPredecessors.Count > 0)
            {
                start = requiredPredecessors
                    .Select(predecessorId => schedule[predecessorId].End)
                    .Max()
                    .AddDays(1);
            }

            var end = AddWorkingDuration(start, durations[activityId]);
            schedule[activityId] = (start, end);
        }

        return schedule;
    }

    private static Dictionary<int, HashSet<int>> BuildPredecessorMap(IReadOnlyList<PlanningActivitySnapshot> activities)
    {
        var predecessors = activities.ToDictionary(activity => activity.Id, _ => new HashSet<int>());

        foreach (var activity in activities)
        {
            foreach (var relation in activity.Relations)
            {
                switch (relation.Type)
                {
                    case ActivityRelationType.Predecessor:
                        predecessors[activity.Id].Add(relation.RelatedActivityId);
                        break;
                    case ActivityRelationType.Successor:
                        predecessors[relation.RelatedActivityId].Add(activity.Id);
                        break;
                }
            }
        }

        return predecessors;
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
            var remaining = activities
                .Select(activity => activity.Id)
                .Except(ordered)
                .OrderBy(id => id);
            ordered.AddRange(remaining);
        }

        return ordered;
    }

    private static GanttActivityRowDto ToActivityRow(
        PlanningActivitySnapshot activity,
        (DateOnly Start, DateOnly End) schedule)
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

    private static DateOnly AddWorkingDuration(DateOnly start, decimal durationDays)
    {
        var spanDays = (int)Math.Max(1, Math.Ceiling(durationDays));
        return start.AddDays(spanDays - 1);
    }
}
