using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Core.Import;

public static class ProjectImportIdentityResolver
{
    public sealed record ExistingExternalId(ImportEntityKind EntityKind, int InternalEntityId);

    public static ProjectModel Resolve(
        ProjectModel model,
        IReadOnlyDictionary<string, ExistingExternalId> existingExternalIds)
    {
        var idMap = new Dictionary<int, int>();

        var projectId = ResolveInternalId(
            ImportEntityKind.Project,
            model.Id,
            model.ExternalIds,
            existingExternalIds,
            idMap);

        var components = model.Components
            .Select(component => ResolveComponent(component, existingExternalIds, idMap))
            .ToList();

        return model with { Id = projectId, Components = components };
    }

    private static ComponentModel ResolveComponent(
        ComponentModel component,
        IReadOnlyDictionary<string, ExistingExternalId> existingExternalIds,
        Dictionary<int, int> idMap)
    {
        var componentId = ResolveInternalId(
            ImportEntityKind.Component,
            component.Id,
            component.ExternalIds,
            existingExternalIds,
            idMap);

        var activities = component.Activities
            .Select(activity => ResolveActivity(activity, existingExternalIds, idMap))
            .ToList();

        var children = component.ChildComponents
            .Select(child => ResolveComponent(child, existingExternalIds, idMap))
            .ToList();

        return component with
        {
            Id = componentId,
            Activities = activities,
            ChildComponents = children
        };
    }

    private static ActivityModel ResolveActivity(
        ActivityModel activity,
        IReadOnlyDictionary<string, ExistingExternalId> existingExternalIds,
        Dictionary<int, int> idMap)
    {
        var activityId = ResolveInternalId(
            ImportEntityKind.Activity,
            activity.Id,
            activity.ExternalIds,
            existingExternalIds,
            idMap);

        var assignments = activity.Assignments
            .Select(assignment =>
            {
                var assignmentId = ResolveInternalId(
                    ImportEntityKind.Assignment,
                    assignment.Id,
                    assignment.ExternalIds,
                    existingExternalIds,
                    idMap);
                return assignment with { Id = assignmentId };
            })
            .ToList();

        var relations = activity.Relations
            .Select(relation => relation with
            {
                RelatedActivityId = idMap.TryGetValue(relation.RelatedActivityId, out var mapped)
                    ? mapped
                    : relation.RelatedActivityId
            })
            .ToList();

        return activity with { Id = activityId, Assignments = assignments, Relations = relations };
    }

    private static int ResolveInternalId(
        ImportEntityKind entityKind,
        int proposedId,
        IReadOnlyDictionary<string, string> externalIds,
        IReadOnlyDictionary<string, ExistingExternalId> existingExternalIds,
        Dictionary<int, int> idMap)
    {
        int? resolvedId = null;

        foreach (var (system, value) in externalIds)
        {
            var composite = ExternalIdHelper.CompositeKey(system, value);
            if (!existingExternalIds.TryGetValue(composite, out var existing))
            {
                continue;
            }

            if (existing.EntityKind != entityKind)
            {
                throw new InvalidOperationException(
                    $"External id '{system}:{value}' already belongs to a different entity type ({existing.EntityKind}).");
            }

            if (resolvedId is not null && resolvedId != existing.InternalEntityId)
            {
                throw new InvalidOperationException(
                    $"External ids for the same entity resolve to different internal ids ('{system}:{value}').");
            }

            resolvedId = existing.InternalEntityId;
        }

        var finalId = resolvedId ?? proposedId;
        idMap[proposedId] = finalId;
        return finalId;
    }
}
