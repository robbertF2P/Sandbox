using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Data;
using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Templates;

public sealed class ComponentTemplateService(IDbContextFactory<ImportDbContext> dbContextFactory)
{
    public async Task<ComponentModel?> SetTemplateAsync(
        int componentId,
        bool isTemplate,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var component = await db.Components.FirstOrDefaultAsync(
            entity => entity.Id == componentId,
            cancellationToken);

        if (component is null)
        {
            return null;
        }

        component.IsTemplate = isTemplate;
        await db.SaveChangesAsync(cancellationToken);

        return await LoadComponentModelAsync(db, componentId, cancellationToken);
    }

    public async Task<IReadOnlyList<ComponentTemplateSummary>> ListTemplatesAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var templates = await db.Components
            .AsNoTracking()
            .Where(component => component.ProjectId == projectId && component.IsTemplate)
            .Include(component => component.Activities)
                .ThenInclude(activity => activity.Assignments)
            .OrderBy(component => component.Name)
            .ToListAsync(cancellationToken);

        return templates
            .Select(component => new ComponentTemplateSummary(
                component.Id,
                component.Name,
                component.Activities.Count,
                component.Activities.Sum(activity => activity.Assignments.Count)))
            .ToList();
    }

    public async Task<InstantiateComponentFromTemplateResult?> InstantiateAsync(
        int templateComponentId,
        InstantiateComponentFromTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var template = await db.Components
            .Include(component => component.Activities)
                .ThenInclude(activity => activity.Assignments)
            .Include(component => component.Activities)
                .ThenInclude(activity => activity.OutgoingRelations)
            .FirstOrDefaultAsync(component => component.Id == templateComponentId, cancellationToken);

        if (template is null || !template.IsTemplate)
        {
            return null;
        }

        if (request.ParentComponentId is int parentComponentId)
        {
            var parentExists = await db.Components.AnyAsync(
                component => component.Id == parentComponentId && component.ProjectId == template.ProjectId,
                cancellationToken);

            if (!parentExists)
            {
                throw new InvalidOperationException(
                    $"Parent component {parentComponentId} was not found in project {template.ProjectId}.");
            }
        }

        var newComponent = new ComponentEntity
        {
            ProjectId = template.ProjectId,
            ParentComponentId = request.ParentComponentId,
            Name = request.Name.Trim(),
            IsTemplate = false
        };

        db.Components.Add(newComponent);
        await db.SaveChangesAsync(cancellationToken);

        var activityIdMap = new Dictionary<int, int>();
        var assignmentCount = 0;

        foreach (var templateActivity in template.Activities.OrderBy(activity => activity.Id))
        {
            var activity = new ActivityEntity
            {
                ComponentId = newComponent.Id,
                Name = templateActivity.Name
            };

            db.Activities.Add(activity);
            await db.SaveChangesAsync(cancellationToken);
            activityIdMap[templateActivity.Id] = activity.Id;

            foreach (var templateAssignment in templateActivity.Assignments)
            {
                db.Assignments.Add(new AssignmentEntity
                {
                    ActivityId = activity.Id,
                    PersonName = string.Empty,
                    Description = templateAssignment.Description,
                    BudgetedHours = templateAssignment.BudgetedHours
                });
                assignmentCount++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var templateActivity in template.Activities)
        {
            if (!activityIdMap.TryGetValue(templateActivity.Id, out var sourceActivityId))
            {
                continue;
            }

            foreach (var relation in templateActivity.OutgoingRelations)
            {
                if (!activityIdMap.TryGetValue(relation.TargetActivityId, out var targetActivityId))
                {
                    continue;
                }

                db.ActivityRelations.Add(new ActivityRelationEntity
                {
                    SourceActivityId = sourceActivityId,
                    TargetActivityId = targetActivityId,
                    RelationType = relation.RelationType
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return new InstantiateComponentFromTemplateResult(
            newComponent.Id,
            activityIdMap.Count,
            assignmentCount);
    }

    private static async Task<ComponentModel?> LoadComponentModelAsync(
        ImportDbContext db,
        int componentId,
        CancellationToken cancellationToken)
    {
        var component = await db.Components
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == componentId, cancellationToken);

        if (component is null)
        {
            return null;
        }

        var components = await db.Components
            .AsNoTracking()
            .Where(entity => entity.ProjectId == component.ProjectId)
            .Include(entity => entity.Activities)
                .ThenInclude(activity => activity.Assignments)
            .Include(entity => entity.Activities)
                .ThenInclude(activity => activity.OutgoingRelations)
            .ToListAsync(cancellationToken);

        var externalIds = await ExternalIdLoader.LoadByInternalIdAsync(db, cancellationToken);
        var roots = components
            .Where(entity => entity.ParentComponentId is null)
            .Select(entity => ProjectEntityReader.ToComponentModel(entity, components, externalIds))
            .ToList();

        return FindComponent(roots, componentId);
    }

    private static ComponentModel? FindComponent(IReadOnlyList<ComponentModel> components, int componentId)
    {
        foreach (var component in components)
        {
            if (component.Id == componentId)
            {
                return component;
            }

            var match = FindComponent(component.ChildComponents, componentId);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }
}
