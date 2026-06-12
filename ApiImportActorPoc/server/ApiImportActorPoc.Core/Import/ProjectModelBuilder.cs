using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Import;

namespace ApiImportActorPoc.Core.Import;

public static class ProjectModelBuilder
{
    public sealed record BuildResult(ProjectModel Model, int TotalSteps);

    public sealed record BuildProgress(int Step, int TotalSteps, string Message);

    private sealed record PendingRelation(int SourceActivityId, ActivityRelationImportPayload Relation);

    private sealed class BuildProgressTracker(int totalSteps, Action<BuildProgress>? onProgress)
    {
        private int _step;

        public void Report(string message) => onProgress?.Invoke(new BuildProgress(++_step, totalSteps, message));
    }

    public static BuildResult Build(ProjectImportPayload payload, Action<BuildProgress>? onProgress = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload.Name);

        if (payload.Components is null || payload.Components.Count == 0)
        {
            throw new ArgumentException("At least one component is required.", nameof(payload));
        }

        ExternalIdUniquenessValidator.ValidateImportPayload(payload);

        var totalSteps = CountSteps(payload) + 1;
        var progress = new BuildProgressTracker(totalSteps, onProgress);
        var tempIds = new TempIdAllocator();
        var activityReferences = new ActivityReferenceIndex();
        var pendingRelations = new List<PendingRelation>();

        progress.Report("Creating project");

        var projectId = tempIds.Next();
        var projectExternalIds = ExternalIdHelper.Normalize(payload.ExternalIds);
        var components = payload.Components
            .Select(component => BuildComponent(component, progress, tempIds, activityReferences, pendingRelations))
            .ToList();

        progress.Report("Validating activity relations");

        var resolvedRelations = ValidateAndResolveRelations(activityReferences, pendingRelations);
        var modelWithRelations = AttachRelations(
            new ProjectModel(projectId, payload.Name.Trim(), components, projectExternalIds),
            resolvedRelations);

        ExternalIdUniquenessValidator.ValidateModel(modelWithRelations);
        return new BuildResult(modelWithRelations, totalSteps);
    }

    private static int CountSteps(ProjectImportPayload payload)
    {
        var count = 1;
        foreach (var component in payload.Components)
        {
            count += CountComponentSteps(component);
        }

        return count + 1;
    }

    private static int CountComponentSteps(ComponentImportPayload component)
    {
        var count = 1;
        if (component.Activities is not null)
        {
            count += component.Activities.Count;
        }

        if (component.ChildComponents is not null)
        {
            foreach (var child in component.ChildComponents)
            {
                count += CountComponentSteps(child);
            }
        }

        return count;
    }

    private static ComponentModel BuildComponent(
        ComponentImportPayload payload,
        BuildProgressTracker progress,
        TempIdAllocator tempIds,
        ActivityReferenceIndex activityReferences,
        List<PendingRelation> pendingRelations)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload.Name);

        progress.Report($"Building component '{payload.Name.Trim()}'");

        var componentId = tempIds.Next();
        var externalIds = ExternalIdHelper.Normalize(payload.ExternalIds);
        var childComponents = payload.ChildComponents?
            .Select(child => BuildComponent(child, progress, tempIds, activityReferences, pendingRelations))
            .ToList() ?? [];

        var activities = new List<ActivityModel>();
        if (payload.Activities is not null)
        {
            foreach (var activityPayload in payload.Activities)
            {
                progress.Report($"Building activity '{activityPayload.Name.Trim()}'");
                activities.Add(BuildActivity(activityPayload, tempIds, activityReferences, pendingRelations));
            }
        }

        return new ComponentModel(componentId, payload.Name.Trim(), childComponents, activities, externalIds);
    }

    private static ActivityModel BuildActivity(
        ActivityImportPayload payload,
        TempIdAllocator tempIds,
        ActivityReferenceIndex activityReferences,
        List<PendingRelation> pendingRelations)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload.Name);

        var activityId = tempIds.Next();
        var externalIds = ExternalIdHelper.Normalize(payload.ExternalIds);
        activityReferences.Register(activityId, payload.Id, externalIds);

        var assignments = payload.Assignments?
            .Select(assignment =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(assignment.PersonName);
                return new AssignmentModel(
                    tempIds.Next(),
                    assignment.PersonName.Trim(),
                    assignment.Description?.Trim(),
                    assignment.BudgetedHours ?? 0,
                    ExternalIdHelper.Normalize(assignment.ExternalIds));
            })
            .ToList() ?? [];

        if (payload.Relations is not null)
        {
            foreach (var relation in payload.Relations)
            {
                pendingRelations.Add(new PendingRelation(activityId, relation));
            }
        }

        return new ActivityModel(activityId, payload.Name.Trim(), assignments, [], externalIds);
    }

    private static Dictionary<int, List<ActivityRelationModel>> ValidateAndResolveRelations(
        ActivityReferenceIndex activityReferences,
        List<PendingRelation> pendingRelations)
    {
        var resolved = new Dictionary<int, List<ActivityRelationModel>>();

        foreach (var pending in pendingRelations)
        {
            if (string.IsNullOrWhiteSpace(pending.Relation.RelatedActivityId))
            {
                throw new ArgumentException("Related activity id is required for relations.");
            }

            if (!TryResolveActivityReference(activityReferences, pending.Relation.RelatedActivityId, out var relatedId))
            {
                throw new InvalidOperationException(
                    $"Activity '{pending.SourceActivityId}' references unknown activity '{pending.Relation.RelatedActivityId}'.");
            }

            if (!Enum.TryParse<ActivityRelationType>(pending.Relation.Type, ignoreCase: true, out var relationType))
            {
                throw new ArgumentException(
                    $"Unknown relation type '{pending.Relation.Type}'. Expected Child, Predecessor, or Successor.");
            }

            if (relationType == ActivityRelationType.Child && relatedId == pending.SourceActivityId)
            {
                throw new InvalidOperationException("An activity cannot be a child of itself.");
            }

            if (!resolved.TryGetValue(pending.SourceActivityId, out var relations))
            {
                relations = [];
                resolved[pending.SourceActivityId] = relations;
            }

            relations.Add(new ActivityRelationModel(relatedId, relationType));
        }

        return resolved;
    }

    private static bool TryResolveActivityReference(
        ActivityReferenceIndex activityReferences,
        string reference,
        out int activityId)
    {
        var trimmed = reference.Trim();
        if (activityReferences.TryResolve(trimmed, out activityId))
        {
            return true;
        }

        var separatorIndex = trimmed.IndexOf(':');
        if (separatorIndex > 0 && separatorIndex < trimmed.Length - 1)
        {
            var composite = ExternalIdHelper.CompositeKey(
                trimmed[..separatorIndex],
                trimmed[(separatorIndex + 1)..]);
            return activityReferences.TryResolve(composite, out activityId);
        }

        return false;
    }

    private static ProjectModel AttachRelations(
        ProjectModel project,
        Dictionary<int, List<ActivityRelationModel>> relationsByActivity)
    {
        var components = project.Components
            .Select(component => AttachComponentRelations(component, relationsByActivity))
            .ToList();

        return project with { Components = components };
    }

    private static ComponentModel AttachComponentRelations(
        ComponentModel component,
        Dictionary<int, List<ActivityRelationModel>> relationsByActivity)
    {
        var activities = component.Activities
            .Select(activity =>
            {
                relationsByActivity.TryGetValue(activity.Id, out var relations);
                return activity with { Relations = relations ?? [] };
            })
            .ToList();

        var children = component.ChildComponents
            .Select(child => AttachComponentRelations(child, relationsByActivity))
            .ToList();

        return component with { Activities = activities, ChildComponents = children };
    }
}
