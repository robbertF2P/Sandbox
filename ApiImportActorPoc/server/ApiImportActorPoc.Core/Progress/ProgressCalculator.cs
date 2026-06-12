using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Progress;
using ApiImportActorPoc.Contracts.Values;
using ApiImportActorPoc.Data.Entities;

namespace ApiImportActorPoc.Core.Progress;

public static class ProgressCalculator
{
    public static ProgressSummary Sum(IEnumerable<ProgressSummary> summaries)
    {
        var budgeted = Hours.Zero;
        var worked = Hours.Zero;
        foreach (var summary in summaries)
        {
            budgeted += summary.BudgetedHours;
            worked += summary.HoursWorked;
        }

        return new ProgressSummary(budgeted, worked);
    }

    public static ProjectProgressDto ToProjectProgress(
        ProjectEntity project,
        IReadOnlyList<ComponentEntity> allComponents)
    {
        var roots = allComponents
            .Where(component => component.ParentComponentId is null)
            .Select(component => ToComponentProgress(component, allComponents))
            .ToList();

        return new ProjectProgressDto(
            project.Id,
            project.Name,
            Sum(roots.Select(root => root.Progress)),
            roots);
    }

    public static ComponentProgressDto ToComponentProgress(
        ComponentEntity component,
        IReadOnlyList<ComponentEntity> allComponents)
    {
        var children = allComponents
            .Where(child => child.ParentComponentId == component.Id)
            .Select(child => ToComponentProgress(child, allComponents))
            .ToList();

        var activities = component.Activities
            .Select(ToActivityProgress)
            .ToList();

        var rollup = children.Select(child => child.Progress)
            .Concat(activities.Select(activity => activity.Progress));

        return new ComponentProgressDto(
            component.Id,
            component.Name,
            Sum(rollup),
            children,
            activities);
    }

    public static ActivityProgressDto ToActivityProgress(ActivityEntity activity)
    {
        var assignments = activity.Assignments
            .Select(ToAssignmentProgress)
            .ToList();

        return new ActivityProgressDto(
            activity.Id,
            activity.Name,
            Sum(assignments.Select(assignment => assignment.Progress)),
            assignments);
    }

    public static AssignmentProgressDto ToAssignmentProgress(AssignmentEntity assignment)
    {
        var hoursWorked = assignment.HourBookings.Aggregate(Hours.Zero, (total, booking) => total + booking.Hours);
        return new AssignmentProgressDto(
            assignment.Id,
            assignment.PersonName.DisplayLabel,
            assignment.Description,
            new ProgressSummary(assignment.BudgetedHours, hoursWorked));
    }
}
