using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Import;

namespace ApiImportActorPoc.Core.Import;

public static class ExternalIdUniquenessValidator
{
    public sealed record ExternalIdOwner(ImportEntityKind Kind, string Path);

    public static void ValidateImportPayload(ProjectImportPayload payload)
    {
        var seen = new Dictionary<string, ExternalIdOwner>(StringComparer.OrdinalIgnoreCase);
        ValidateEntity(
            ImportEntityKind.Project,
            "project",
            ExternalIdHelper.Normalize(payload.ExternalIds),
            seen);
        ValidateComponents(payload.Components, "project.components", seen);
    }

    public static void ValidateModel(ProjectModel model)
    {
        var seen = new Dictionary<string, ExternalIdOwner>(StringComparer.OrdinalIgnoreCase);
        ValidateEntity(ImportEntityKind.Project, "project", model.ExternalIds, seen);
        ValidateModelComponents(model.Components, "project.components", seen);
    }

    private static void ValidateComponents(
        IReadOnlyList<ComponentImportPayload> components,
        string path,
        Dictionary<string, ExternalIdOwner> seen)
    {
        for (var index = 0; index < components.Count; index++)
        {
            var component = components[index];
            var componentPath = $"{path}[{index}]";
            ValidateEntity(
                ImportEntityKind.Component,
                componentPath,
                ExternalIdHelper.Normalize(component.ExternalIds),
                seen);

            if (component.ChildComponents is not null)
            {
                ValidateComponents(component.ChildComponents, $"{componentPath}.childComponents", seen);
            }

            if (component.Activities is not null)
            {
                ValidateActivities(component.Activities, $"{componentPath}.activities", seen);
            }
        }
    }

    private static void ValidateActivities(
        IReadOnlyList<ActivityImportPayload> activities,
        string path,
        Dictionary<string, ExternalIdOwner> seen)
    {
        for (var index = 0; index < activities.Count; index++)
        {
            var activity = activities[index];
            var activityPath = $"{path}[{index}]";
            ValidateEntity(
                ImportEntityKind.Activity,
                activityPath,
                ExternalIdHelper.Normalize(activity.ExternalIds),
                seen);

            if (activity.Assignments is not null)
            {
                ValidateAssignments(activity.Assignments, $"{activityPath}.assignments", seen);
            }
        }
    }

    private static void ValidateAssignments(
        IReadOnlyList<AssignmentImportPayload> assignments,
        string path,
        Dictionary<string, ExternalIdOwner> seen)
    {
        for (var index = 0; index < assignments.Count; index++)
        {
            var assignment = assignments[index];
            ValidateEntity(
                ImportEntityKind.Assignment,
                $"{path}[{index}]",
                ExternalIdHelper.Normalize(assignment.ExternalIds),
                seen);
        }
    }

    private static void ValidateModelComponents(
        IReadOnlyList<ComponentModel> components,
        string path,
        Dictionary<string, ExternalIdOwner> seen)
    {
        for (var index = 0; index < components.Count; index++)
        {
            var component = components[index];
            var componentPath = $"{path}[{index}]";
            ValidateEntity(ImportEntityKind.Component, componentPath, component.ExternalIds, seen);
            ValidateModelComponents(component.ChildComponents, $"{componentPath}.childComponents", seen);

            for (var activityIndex = 0; activityIndex < component.Activities.Count; activityIndex++)
            {
                var activity = component.Activities[activityIndex];
                var activityPath = $"{componentPath}.activities[{activityIndex}]";
                ValidateEntity(ImportEntityKind.Activity, activityPath, activity.ExternalIds, seen);

                for (var assignmentIndex = 0; assignmentIndex < activity.Assignments.Count; assignmentIndex++)
                {
                    ValidateEntity(
                        ImportEntityKind.Assignment,
                        $"{activityPath}.assignments[{assignmentIndex}]",
                        activity.Assignments[assignmentIndex].ExternalIds,
                        seen);
                }
            }
        }
    }

    private static void ValidateEntity(
        ImportEntityKind kind,
        string path,
        IReadOnlyDictionary<string, string> externalIds,
        Dictionary<string, ExternalIdOwner> seen)
    {
        foreach (var (system, value) in externalIds)
        {
            var composite = ExternalIdHelper.CompositeKey(system, value);
            if (seen.TryGetValue(composite, out var existing))
            {
                throw new InvalidOperationException(
                    $"External id '{system}:{value}' is duplicated between {existing.Path} and {path}.");
            }

            seen[composite] = new ExternalIdOwner(kind, path);
        }
    }
}
