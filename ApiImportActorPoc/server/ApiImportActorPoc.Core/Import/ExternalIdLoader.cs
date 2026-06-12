using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Data;
using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Import;

public static class ExternalIdLoader
{
    public static async Task<IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, string>>> LoadByInternalIdAsync(
        ImportDbContext db,
        CancellationToken cancellationToken = default)
    {
        var rows = await db.EntityExternalIds.AsNoTracking().ToListAsync(cancellationToken);
        return rows
            .GroupBy(row => row.InternalEntityId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyDictionary<string, string>)group.ToDictionary(
                    row => row.System,
                    row => row.Value,
                    StringComparer.OrdinalIgnoreCase));
    }

    public static IReadOnlyDictionary<string, string> ForEntity(
        Guid internalEntityId,
        IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, string>> lookup) =>
        lookup.TryGetValue(internalEntityId, out var externalIds)
            ? externalIds
            : new Dictionary<string, string>();
}
