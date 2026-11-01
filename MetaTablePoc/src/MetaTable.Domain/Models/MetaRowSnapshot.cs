namespace MetaTable.Domain.Models;

public sealed class MetaRowSnapshot
{
    public Guid RowId { get; init; }

    public required string TableKey { get; init; }

    public IReadOnlyDictionary<string, string?> TextValues { get; init; } =
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, int?> IntegerValues { get; init; } =
        new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, Guid?> AssociationValues { get; init; } =
        new Dictionary<string, Guid?>(StringComparer.OrdinalIgnoreCase);
}
