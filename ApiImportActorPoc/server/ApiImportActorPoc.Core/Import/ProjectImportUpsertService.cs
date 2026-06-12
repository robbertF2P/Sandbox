using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Data;
using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Import;

public sealed class ProjectImportUpsertService(IDbContextFactory<ImportDbContext> dbContextFactory)
{
    public sealed record UpsertResult(Guid ProjectId, bool Created);

    public async Task<UpsertResult> UpsertAsync(ProjectModel model, CancellationToken cancellationToken = default)
    {
        ExternalIdUniquenessValidator.ValidateModel(model);

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var existingExternalIds = await LoadExistingExternalIdsAsync(db, cancellationToken);
        var resolvedModel = ProjectImportIdentityResolver.Resolve(model, existingExternalIds);

        var created = !await db.Projects.AnyAsync(
            project => project.Id == resolvedModel.Id,
            cancellationToken);

        var project = await db.Projects
            .Include(projectEntity => projectEntity.Components)
                .ThenInclude(component => component.Activities)
                    .ThenInclude(activity => activity.Assignments)
            .Include(projectEntity => projectEntity.Components)
                .ThenInclude(component => component.Activities)
                    .ThenInclude(activity => activity.OutgoingRelations)
            .FirstOrDefaultAsync(projectEntity => projectEntity.Id == resolvedModel.Id, cancellationToken);

        if (project is null)
        {
            project = new ProjectEntity { Id = resolvedModel.Id, Name = resolvedModel.Name };
            db.Projects.Add(project);
        }
        else
        {
            project.Name = resolvedModel.Name;
        }

        var allComponents = await db.Components
            .Where(component => component.ProjectId == resolvedModel.Id)
            .Include(component => component.Activities)
                .ThenInclude(activity => activity.Assignments)
            .Include(component => component.Activities)
                .ThenInclude(activity => activity.OutgoingRelations)
            .ToListAsync(cancellationToken);

        var componentsById = allComponents.ToDictionary(component => component.Id);
        UpsertComponents(
            db,
            resolvedModel.Id,
            parentComponentId: null,
            resolvedModel.Components,
            componentsById);

        await ReplaceExternalIdsAsync(db, resolvedModel, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new UpsertResult(resolvedModel.Id, created);
    }

    private static async Task<Dictionary<string, ProjectImportIdentityResolver.ExistingExternalId>> LoadExistingExternalIdsAsync(
        ImportDbContext db,
        CancellationToken cancellationToken)
    {
        var rows = await db.EntityExternalIds.AsNoTracking().ToListAsync(cancellationToken);
        return rows.ToDictionary(
            row => ExternalIdHelper.CompositeKey(row.System, row.Value),
            row => new ProjectImportIdentityResolver.ExistingExternalId(row.EntityKind, row.InternalEntityId),
            StringComparer.OrdinalIgnoreCase);
    }

    private static void UpsertComponents(
        ImportDbContext db,
        Guid projectId,
        Guid? parentComponentId,
        IReadOnlyList<ComponentModel> components,
        Dictionary<Guid, ComponentEntity> componentsById)
    {
        foreach (var componentModel in components)
        {
            if (!componentsById.TryGetValue(componentModel.Id, out var component))
            {
                component = new ComponentEntity
                {
                    Id = componentModel.Id,
                    ProjectId = projectId,
                    ParentComponentId = parentComponentId,
                    Name = componentModel.Name
                };
                db.Components.Add(component);
                componentsById[componentModel.Id] = component;
            }
            else
            {
                component.Name = componentModel.Name;
                component.ParentComponentId = parentComponentId;
                component.ProjectId = projectId;
            }

            UpsertActivities(component, componentModel.Activities);
            UpsertComponents(
                db,
                projectId,
                componentModel.Id,
                componentModel.ChildComponents,
                componentsById);
        }
    }

    private static void UpsertActivities(
        ComponentEntity component,
        IReadOnlyList<ActivityModel> activities)
    {
        var activitiesById = component.Activities.ToDictionary(activity => activity.Id);

        foreach (var activityModel in activities)
        {
            if (!activitiesById.TryGetValue(activityModel.Id, out var activity))
            {
                activity = new ActivityEntity
                {
                    Id = activityModel.Id,
                    ComponentId = component.Id,
                    Name = activityModel.Name
                };
                component.Activities.Add(activity);
                activitiesById[activityModel.Id] = activity;
            }
            else
            {
                activity.Name = activityModel.Name;
                activity.ComponentId = component.Id;
            }

            UpsertAssignments(activity, activityModel.Assignments);
            ReplaceActivityRelations(activity, activityModel.Relations);
        }
    }

    private static void UpsertAssignments(ActivityEntity activity, IReadOnlyList<AssignmentModel> assignments)
    {
        var assignmentsById = activity.Assignments.ToDictionary(assignment => assignment.Id);

        foreach (var assignmentModel in assignments)
        {
            if (!assignmentsById.TryGetValue(assignmentModel.Id, out var assignment))
            {
                assignment = new AssignmentEntity
                {
                    Id = assignmentModel.Id,
                    ActivityId = activity.Id,
                    PersonName = assignmentModel.PersonName,
                    Description = assignmentModel.Description
                };
                activity.Assignments.Add(assignment);
                assignmentsById[assignmentModel.Id] = assignment;
            }
            else
            {
                assignment.PersonName = assignmentModel.PersonName;
                assignment.Description = assignmentModel.Description;
                assignment.ActivityId = activity.Id;
            }
        }
    }

    private static void ReplaceActivityRelations(
        ActivityEntity activity,
        IReadOnlyList<ActivityRelationModel> relations)
    {
        activity.OutgoingRelations.Clear();
        foreach (var relation in relations)
        {
            activity.OutgoingRelations.Add(new ActivityRelationEntity
            {
                Id = Guid.NewGuid(),
                SourceActivityId = activity.Id,
                TargetActivityId = relation.RelatedActivityId,
                RelationType = relation.Type.ToString()
            });
        }
    }

    private static async Task ReplaceExternalIdsAsync(
        ImportDbContext db,
        ProjectModel model,
        CancellationToken cancellationToken)
    {
        var internalIds = CollectInternalIds(model);
        var existingRows = await db.EntityExternalIds
            .Where(externalId => internalIds.Contains(externalId.InternalEntityId))
            .ToListAsync(cancellationToken);

        if (existingRows.Count > 0)
        {
            db.EntityExternalIds.RemoveRange(existingRows);
        }

        AddExternalIds(db, ImportEntityKind.Project, model.Id, model.ExternalIds);
        AddComponentExternalIds(db, model.Components);
    }

    private static HashSet<Guid> CollectInternalIds(ProjectModel model)
    {
        var ids = new HashSet<Guid> { model.Id };
        CollectComponentIds(model.Components, ids);
        return ids;
    }

    private static void CollectComponentIds(IReadOnlyList<ComponentModel> components, HashSet<Guid> ids)
    {
        foreach (var component in components)
        {
            ids.Add(component.Id);
            foreach (var activity in component.Activities)
            {
                ids.Add(activity.Id);
                foreach (var assignment in activity.Assignments)
                {
                    ids.Add(assignment.Id);
                }
            }

            CollectComponentIds(component.ChildComponents, ids);
        }
    }

    private static void AddComponentExternalIds(ImportDbContext db, IReadOnlyList<ComponentModel> components)
    {
        foreach (var component in components)
        {
            AddExternalIds(db, ImportEntityKind.Component, component.Id, component.ExternalIds);
            foreach (var activity in component.Activities)
            {
                AddExternalIds(db, ImportEntityKind.Activity, activity.Id, activity.ExternalIds);
                foreach (var assignment in activity.Assignments)
                {
                    AddExternalIds(db, ImportEntityKind.Assignment, assignment.Id, assignment.ExternalIds);
                }
            }

            AddComponentExternalIds(db, component.ChildComponents);
        }
    }

    private static void AddExternalIds(
        ImportDbContext db,
        ImportEntityKind entityKind,
        Guid internalEntityId,
        IReadOnlyDictionary<string, string> externalIds)
    {
        foreach (var (system, value) in externalIds)
        {
            db.EntityExternalIds.Add(new EntityExternalIdEntity
            {
                Id = Guid.NewGuid(),
                System = system,
                Value = value,
                EntityKind = entityKind,
                InternalEntityId = internalEntityId
            });
        }
    }
}
