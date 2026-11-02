using MetaTable.Domain.Enums;

namespace MetaTable.Domain.Models;

public sealed class MetaColumnDefinition
{
    public Guid Id { get; init; }

    public Guid TableId { get; init; }

    public required string Name { get; init; }

    public MetaColumnKind Kind { get; init; }

    public Guid? AssociationTargetTableId { get; init; }

    public bool IsRequired { get; init; }

    public int SortOrder { get; init; }
}
