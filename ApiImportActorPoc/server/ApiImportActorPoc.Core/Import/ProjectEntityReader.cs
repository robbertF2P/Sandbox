using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Data.Entities;

namespace ApiImportActorPoc.Core.Import;

public static class ProjectEntityReader
{
    public static ProjectModel ToModel(ProjectEntity project, IReadOnlyList<ComponentEntity> allComponents)
    {
        var roots = allComponents
            .Where(component => component.ParentComponentId is null)
            .Select(component => ToComponentModel(component, allComponents))
            .ToList();

        return new ProjectModel(project.Id, project.Name, roots);
    }

    public static ProjectImportPayload ToImportPayload(ProjectModel model) =>
        new(model.Name, model.Components.Select(ToComponentPayload).ToList());

    private static ComponentModel ToComponentModel(
        ComponentEntity component,
        IReadOnlyList<ComponentEntity> allComponents)
    {
        var children = allComponents
            .Where(child => child.ParentComponentId == component.Id)
            .Select(child => ToComponentModel(child, allComponents))
            .ToList();

        var activities = component.Activities
            .Select(ToActivityModel)
            .ToList();

        return new ComponentModel(component.Id, component.Name, children, activities);
    }

    private static ActivityModel ToActivityModel(ActivityEntity activity)
    {
        var assignments = activity.Assignments
            .Select(assignment => new AssignmentModel(
                assignment.Id,
                assignment.PersonName,
                assignment.Description))
            .ToList();

        var relations = activity.OutgoingRelations
            .Select(relation => new ActivityRelationModel(
                relation.TargetActivityId,
                Enum.Parse<ActivityRelationType>(relation.RelationType, ignoreCase: true)))
            .ToList();

        return new ActivityModel(activity.Id, activity.Name, assignments, relations);
    }

    private static ComponentImportPayload ToComponentPayload(ComponentModel model) =>
        new(
            model.Id.ToString(),
            model.Name,
            model.ChildComponents.Count > 0
                ? model.ChildComponents.Select(ToComponentPayload).ToList()
                : null,
            model.Activities.Count > 0
                ? model.Activities.Select(ToActivityPayload).ToList()
                : null);

    private static ActivityImportPayload ToActivityPayload(ActivityModel model) =>
        new(
            model.Id.ToString(),
            model.Name,
            model.Assignments.Count > 0
                ? model.Assignments.Select(assignment => new AssignmentImportPayload(
                    assignment.Id.ToString(),
                    assignment.PersonName,
                    assignment.Description)).ToList()
                : null,
            model.Relations.Count > 0
                ? model.Relations.Select(relation => new ActivityRelationImportPayload(
                    relation.RelatedActivityId.ToString(),
                    relation.Type.ToString())).ToList()
                : null);
}
