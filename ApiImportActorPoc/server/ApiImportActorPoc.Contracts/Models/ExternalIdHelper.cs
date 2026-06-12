namespace ApiImportActorPoc.Contracts.Models;

public static class ExternalIdHelper
{
    public static IReadOnlyDictionary<string, string> Normalize(
        IReadOnlyDictionary<string, string>? externalIds)
    {
        if (externalIds is null || externalIds.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (system, value) in externalIds)
        {
            if (string.IsNullOrWhiteSpace(system))
            {
                throw new ArgumentException("External id system key is required.");
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"External id value is required for system '{system}'.");
            }

            var key = system.Trim();
            var normalizedValue = value.Trim();
            if (normalized.ContainsKey(key))
            {
                throw new ArgumentException($"Duplicate external id system '{key}' on the same entity.");
            }

            normalized[key] = normalizedValue;
        }

        return normalized;
    }

    public static string CompositeKey(string system, string value) => $"{system.Trim()}\u001f{value.Trim()}";
}
