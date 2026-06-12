using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Data.Entities;

namespace ApiImportActorPoc.Core.Import;

public static class ProjectEntityReader
{
    public static ProjectModel ToModel(
        ProjectEntity project,
        IReadOnlyList<ComponentEntity> allComponents,
        IReadOnlyDictionary<int, IReadOnlyDictionary<string, string>> externalIdsByInternalId)
    {
        var roots = allComponents
            .Where(component => component.ParentComponentId is null)
            .Select(component => ToComponentModel(component, allComponents, externalIdsByInternalId))
            .ToList();

        return new ProjectModel(
            project.Id,
            project.Name,
            roots,
            ExternalIdLoader.ForEntity(project.Id, externalIdsByInternalId));
    }

    public static ProjectImportPayload ToImportPayload(ProjectModel model)
    {
        var activityReferences = BuildActivityReferenceLookup(model);
        return new(
            model.Name,
            model.Components.Select(component => ToComponentPayload(component, activityReferences)).ToList(),
            CopyExternalIds(model.ExternalIds));
    }

    private static Dictionary<int, string> BuildActivityReferenceLookup(ProjectModel model)
    {
        var lookup = new Dictionary<int, string>();
        CollectActivityReferences(model.Components, lookup);
        return lookup;
    }

    private static void CollectActivityReferences(
        IReadOnlyList<ComponentModel> components,
        Dictionary<int, string> lookup)
    {
        foreach (var component in components)
        {
            foreach (var activity in component.Activities)
            {
                var preferredReference = activity.ExternalIds.Count > 0
                    ? $"{activity.ExternalIds.First().Key}:{activity.ExternalIds.First().Value}"
                    : activity.Id.ToString();
                lookup[activity.Id] = preferredReference;
            }

            CollectActivityReferences(component.ChildComponents, lookup);
        }
    }

    public static ComponentModel ToComponentModel(
        ComponentEntity component,
        IReadOnlyList<ComponentEntity> allComponents,
        IReadOnlyDictionary<int, IReadOnlyDictionary<string, string>> externalIdsByInternalId)
    {
        var children = allComponents
            .Where(child => child.ParentComponentId == component.Id)
            .Select(child => ToComponentModel(child, allComponents, externalIdsByInternalId))
            .ToList();

        var activities = component.Activities
            .Select(activity => ToActivityModel(activity, externalIdsByInternalId))
            .ToList();

        return new ComponentModel(
            component.Id,
            component.Name,
            component.IsTemplate,
            children,
            activities,
            ExternalIdLoader.ForEntity(component.Id, externalIdsByInternalId));
    }

    private static ActivityModel ToActivityModel(
        ActivityEntity activity,
        IReadOnlyDictionary<int, IReadOnlyDictionary<string, string>> externalIdsByInternalId)
    {
        var assignments = activity.Assignments
            .Select(assignment => new AssignmentModel(
                assignment.Id,
                assignment.PersonName,
                assignment.Description,
                assignment.BudgetedHours,
                ExternalIdLoader.ForEntity(assignment.Id, externalIdsByInternalId)))
            .ToList();

        var relations = activity.OutgoingRelations
            .Select(relation => new ActivityRelationModel(
                relation.TargetActivityId,
                Enum.Parse<ActivityRelationType>(relation.RelationType, ignoreCase: true)))
            .ToList();

        return new ActivityModel(
            activity.Id,
            activity.Name,
            assignments,
            relations,
            ExternalIdLoader.ForEntity(activity.Id, externalIdsByInternalId));
    }

    private static ComponentImportPayload ToComponentPayload(
        ComponentModel model,
        IReadOnlyDictionary<int, string> activityReferences) =>
        new(
            null,
            model.Name,
            model.IsTemplate ? true : null,
            model.ChildComponents.Count > 0
                ? model.ChildComponents.Select(child => ToComponentPayload(child, activityReferences)).ToList()
                : null,
            model.Activities.Count > 0
                ? model.Activities.Select(activity => ToActivityPayload(activity, activityReferences)).ToList()
                : null,
            CopyExternalIds(model.ExternalIds));

    private static ActivityImportPayload ToActivityPayload(
        ActivityModel model,
        IReadOnlyDictionary<int, string> activityReferences) =>
        new(
            null,
            model.Name,
            model.Assignments.Count > 0
                ? model.Assignments.Select(assignment => new AssignmentImportPayload(
                    null,
                    assignment.PersonName,
                    assignment.Description,
                    assignment.BudgetedHours,
                    CopyExternalIds(assignment.ExternalIds))).ToList()
                : null,
            model.Relations.Count > 0
                ? model.Relations.Select(relation => new ActivityRelationImportPayload(
                    activityReferences.TryGetValue(relation.RelatedActivityId, out var reference)
                        ? reference
                        : relation.RelatedActivityId.ToString(),
                    relation.Type.ToString())).ToList()
                : null,
            CopyExternalIds(model.ExternalIds));

    private static IReadOnlyDictionary<string, string>? CopyExternalIds(IReadOnlyDictionary<string, string> externalIds) =>
        externalIds.Count > 0
            ? externalIds.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase)
            : null;
}
