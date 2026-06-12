using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Core.Import;

public sealed class ActivityReferenceIndex
{
    private readonly Dictionary<string, Guid> _references = new(StringComparer.OrdinalIgnoreCase);

    public void Register(Guid activityId, string? legacyId, IReadOnlyDictionary<string, string> externalIds)
    {
        _references[activityId.ToString()] = activityId;

        if (!string.IsNullOrWhiteSpace(legacyId))
        {
            RegisterAlias(legacyId.Trim(), activityId);
        }

        foreach (var (system, value) in externalIds)
        {
            RegisterAlias(ExternalIdHelper.CompositeKey(system, value), activityId);
            RegisterAlias(value, activityId);
        }
    }

    public bool TryResolve(string reference, out Guid activityId) =>
        _references.TryGetValue(reference.Trim(), out activityId);

    private void RegisterAlias(string alias, Guid activityId)
    {
        if (_references.TryGetValue(alias, out var existing) && existing != activityId)
        {
            throw new InvalidOperationException($"Duplicate activity reference alias '{alias}'.");
        }

        _references[alias] = activityId;
    }
}
