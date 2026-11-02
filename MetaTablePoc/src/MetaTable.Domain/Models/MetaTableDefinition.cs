namespace MetaTable.Domain.Models;

public sealed class MetaTableDefinition
{
    public Guid Id { get; init; }

    public required string Key { get; init; }

    public required string DisplayName { get; init; }

    public IReadOnlyList<MetaColumnDefinition> Columns { get; init; } = [];
}
