using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Data.Entities;

namespace ApiImportActorPoc.Core.Import;

public static class ProjectEntityMapper
{
    public static ProjectEntity ToEntity(ProjectModel model)
    {
        var project = new ProjectEntity
        {
            Id = model.Id,
            Name = model.Name
        };

        foreach (var component in model.Components)
        {
            project.Components.Add(ToComponentEntity(component, project.Id, parentComponentId: null));
        }

        return project;
    }

    private static ComponentEntity ToComponentEntity(
        ComponentModel model,
        Guid projectId,
        Guid? parentComponentId)
    {
        var component = new ComponentEntity
        {
            Id = model.Id,
            ProjectId = projectId,
            ParentComponentId = parentComponentId,
            Name = model.Name
        };

        foreach (var child in model.ChildComponents)
        {
            component.ChildComponents.Add(ToComponentEntity(child, projectId, component.Id));
        }

        foreach (var activity in model.Activities)
        {
            component.Activities.Add(ToActivityEntity(activity, component.Id));
        }

        return component;
    }

    private static ActivityEntity ToActivityEntity(ActivityModel model, Guid componentId)
    {
        var activity = new ActivityEntity
        {
            Id = model.Id,
            ComponentId = componentId,
            Name = model.Name
        };

        foreach (var assignment in model.Assignments)
        {
            activity.Assignments.Add(new AssignmentEntity
            {
                Id = assignment.Id,
                ActivityId = activity.Id,
                PersonName = assignment.PersonName,
                Description = assignment.Description
            });
        }

        foreach (var relation in model.Relations)
        {
            activity.OutgoingRelations.Add(new ActivityRelationEntity
            {
                Id = Guid.NewGuid(),
                SourceActivityId = activity.Id,
                TargetActivityId = relation.RelatedActivityId,
                RelationType = relation.Type.ToString()
            });
        }

        return activity;
    }
}
