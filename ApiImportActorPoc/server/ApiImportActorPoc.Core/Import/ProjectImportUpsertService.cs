using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Data;
using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Import;

public sealed class ProjectImportUpsertService(IDbContextFactory<ImportDbContext> dbContextFactory)
{
    public sealed record UpsertResult(int ProjectId, bool Created);

    public async Task<UpsertResult> UpsertAsync(ProjectModel model, CancellationToken cancellationToken = default)
    {
        ExternalIdUniquenessValidator.ValidateModel(model);

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var existingExternalIds = await LoadExistingExternalIdsAsync(db, cancellationToken);
        var resolvedModel = ProjectImportIdentityResolver.Resolve(model, existingExternalIds);
        var idMap = new Dictionary<int, int>();
        var deferredRelations = new List<(int SourceActivityModelId, ActivityRelationModel Relation)>();

        var created = resolvedModel.Id <= 0
                      || !await db.Projects.AnyAsync(project => project.Id == resolvedModel.Id, cancellationToken);

        var projectId = await UpsertProjectAsync(db, resolvedModel, idMap, cancellationToken);
        await UpsertComponentsAsync(
            db,
            projectId,
            parentComponentId: null,
            resolvedModel.Components,
            idMap,
            deferredRelations,
            cancellationToken);

        await ApplyDeferredRelationsAsync(db, idMap, deferredRelations, cancellationToken);
        await ReplaceExternalIdsAsync(db, resolvedModel, idMap, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new UpsertResult(idMap[resolvedModel.Id], created);
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

    private static async Task<int> UpsertProjectAsync(
        ImportDbContext db,
        ProjectModel model,
        Dictionary<int, int> idMap,
        CancellationToken cancellationToken)
    {
        if (model.Id > 0)
        {
            var existing = await db.Projects.FirstAsync(project => project.Id == model.Id, cancellationToken);
            existing.Name = model.Name;
            idMap[model.Id] = existing.Id;
            return existing.Id;
        }

        var project = new ProjectEntity { Name = model.Name };
        db.Projects.Add(project);
        await db.SaveChangesAsync(cancellationToken);
        idMap[model.Id] = project.Id;
        return project.Id;
    }

    private static async Task UpsertComponentsAsync(
        ImportDbContext db,
        int projectId,
        int? parentComponentId,
        IReadOnlyList<ComponentModel> components,
        Dictionary<int, int> idMap,
        List<(int SourceActivityModelId, ActivityRelationModel Relation)> deferredRelations,
        CancellationToken cancellationToken)
    {
        foreach (var componentModel in components)
        {
            var componentId = await UpsertComponentAsync(
                db,
                projectId,
                parentComponentId,
                componentModel,
                idMap,
                cancellationToken);

            await UpsertActivitiesAsync(
                db,
                componentId,
                componentModel.Activities,
                idMap,
                deferredRelations,
                cancellationToken);

            await UpsertComponentsAsync(
                db,
                projectId,
                componentId,
                componentModel.ChildComponents,
                idMap,
                deferredRelations,
                cancellationToken);
        }
    }

    private static async Task<int> UpsertComponentAsync(
        ImportDbContext db,
        int projectId,
        int? parentComponentId,
        ComponentModel model,
        Dictionary<int, int> idMap,
        CancellationToken cancellationToken)
    {
        if (model.Id > 0)
        {
            var existing = await db.Components.FirstAsync(component => component.Id == model.Id, cancellationToken);
            existing.Name = model.Name;
            existing.ProjectId = projectId;
            existing.ParentComponentId = parentComponentId;
            idMap[model.Id] = existing.Id;
            return existing.Id;
        }

        var component = new ComponentEntity
        {
            ProjectId = projectId,
            ParentComponentId = parentComponentId,
            Name = model.Name
        };
        db.Components.Add(component);
        await db.SaveChangesAsync(cancellationToken);
        idMap[model.Id] = component.Id;
        return component.Id;
    }

    private static async Task UpsertActivitiesAsync(
        ImportDbContext db,
        int componentId,
        IReadOnlyList<ActivityModel> activities,
        Dictionary<int, int> idMap,
        List<(int SourceActivityModelId, ActivityRelationModel Relation)> deferredRelations,
        CancellationToken cancellationToken)
    {
        foreach (var activityModel in activities)
        {
            var activityId = await UpsertActivityAsync(db, componentId, activityModel, idMap, cancellationToken);
            await UpsertAssignmentsAsync(db, activityId, activityModel.Assignments, idMap, cancellationToken);

            foreach (var relation in activityModel.Relations)
            {
                deferredRelations.Add((activityModel.Id, relation));
            }
        }
    }

    private static async Task<int> UpsertActivityAsync(
        ImportDbContext db,
        int componentId,
        ActivityModel model,
        Dictionary<int, int> idMap,
        CancellationToken cancellationToken)
    {
        if (model.Id > 0)
        {
            var existing = await db.Activities
                .Include(activity => activity.OutgoingRelations)
                .FirstAsync(activity => activity.Id == model.Id, cancellationToken);
            existing.Name = model.Name;
            existing.ComponentId = componentId;
            if (existing.OutgoingRelations.Count > 0)
            {
                db.ActivityRelations.RemoveRange(existing.OutgoingRelations);
                existing.OutgoingRelations.Clear();
            }

            idMap[model.Id] = existing.Id;
            return existing.Id;
        }

        var activity = new ActivityEntity
        {
            ComponentId = componentId,
            Name = model.Name
        };
        db.Activities.Add(activity);
        await db.SaveChangesAsync(cancellationToken);
        idMap[model.Id] = activity.Id;
        return activity.Id;
    }

    private static async Task UpsertAssignmentsAsync(
        ImportDbContext db,
        int activityId,
        IReadOnlyList<AssignmentModel> assignments,
        Dictionary<int, int> idMap,
        CancellationToken cancellationToken)
    {
        foreach (var assignmentModel in assignments)
        {
            if (assignmentModel.Id > 0)
            {
                var existing = await db.Assignments.FirstAsync(
                    assignment => assignment.Id == assignmentModel.Id,
                    cancellationToken);
                existing.PersonName = assignmentModel.PersonName;
                existing.Description = assignmentModel.Description;
                existing.BudgetedHours = assignmentModel.BudgetedHours;
                existing.ActivityId = activityId;
                idMap[assignmentModel.Id] = existing.Id;
                continue;
            }

            var assignment = new AssignmentEntity
            {
                ActivityId = activityId,
                PersonName = assignmentModel.PersonName,
                Description = assignmentModel.Description,
                BudgetedHours = assignmentModel.BudgetedHours
            };
            db.Assignments.Add(assignment);
            await db.SaveChangesAsync(cancellationToken);
            idMap[assignmentModel.Id] = assignment.Id;
        }
    }

    private static async Task ApplyDeferredRelationsAsync(
        ImportDbContext db,
        Dictionary<int, int> idMap,
        List<(int SourceActivityModelId, ActivityRelationModel Relation)> deferredRelations,
        CancellationToken cancellationToken)
    {
        foreach (var (sourceActivityModelId, relation) in deferredRelations)
        {
            var sourceActivityId = idMap[sourceActivityModelId];
            var targetActivityId = idMap.TryGetValue(relation.RelatedActivityId, out var mapped)
                ? mapped
                : relation.RelatedActivityId;

            db.ActivityRelations.Add(new ActivityRelationEntity
            {
                SourceActivityId = sourceActivityId,
                TargetActivityId = targetActivityId,
                RelationType = relation.Type.ToString()
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task ReplaceExternalIdsAsync(
        ImportDbContext db,
        ProjectModel model,
        Dictionary<int, int> idMap,
        CancellationToken cancellationToken)
    {
        var internalIds = CollectInternalIds(model, idMap);
        var existingRows = await db.EntityExternalIds
            .Where(externalId => internalIds.Contains(externalId.InternalEntityId))
            .ToListAsync(cancellationToken);

        if (existingRows.Count > 0)
        {
            db.EntityExternalIds.RemoveRange(existingRows);
        }

        AddExternalIds(db, ImportEntityKind.Project, idMap[model.Id], model.ExternalIds);
        AddComponentExternalIds(db, model.Components, idMap);
    }

    private static HashSet<int> CollectInternalIds(ProjectModel model, Dictionary<int, int> idMap)
    {
        var ids = new HashSet<int> { idMap[model.Id] };
        CollectComponentIds(model.Components, idMap, ids);
        return ids;
    }

    private static void CollectComponentIds(
        IReadOnlyList<ComponentModel> components,
        Dictionary<int, int> idMap,
        HashSet<int> ids)
    {
        foreach (var component in components)
        {
            ids.Add(idMap[component.Id]);
            foreach (var activity in component.Activities)
            {
                ids.Add(idMap[activity.Id]);
                foreach (var assignment in activity.Assignments)
                {
                    ids.Add(idMap[assignment.Id]);
                }
            }

            CollectComponentIds(component.ChildComponents, idMap, ids);
        }
    }

    private static void AddComponentExternalIds(
        ImportDbContext db,
        IReadOnlyList<ComponentModel> components,
        Dictionary<int, int> idMap)
    {
        foreach (var component in components)
        {
            AddExternalIds(db, ImportEntityKind.Component, idMap[component.Id], component.ExternalIds);
            foreach (var activity in component.Activities)
            {
                AddExternalIds(db, ImportEntityKind.Activity, idMap[activity.Id], activity.ExternalIds);
                foreach (var assignment in activity.Assignments)
                {
                    AddExternalIds(db, ImportEntityKind.Assignment, idMap[assignment.Id], assignment.ExternalIds);
                }
            }

            AddComponentExternalIds(db, component.ChildComponents, idMap);
        }
    }

    private static void AddExternalIds(
        ImportDbContext db,
        ImportEntityKind entityKind,
        int internalEntityId,
        IReadOnlyDictionary<string, string> externalIds)
    {
        foreach (var (system, value) in externalIds)
        {
            db.EntityExternalIds.Add(new EntityExternalIdEntity
            {
                System = system,
                Value = value,
                EntityKind = entityKind,
                InternalEntityId = internalEntityId
            });
        }
    }
}
