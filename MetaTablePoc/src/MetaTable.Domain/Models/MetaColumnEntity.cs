using MetaTable.Domain.Enums;

namespace MetaTable.Domain.Models;

public sealed class MetaColumnEntity
{
    public Guid Id { get; set; }

    public Guid TableId { get; set; }

    public MetaTableEntity Table { get; set; } = null!;

    public required string Name { get; set; }

    public MetaColumnKind Kind { get; set; }

    public Guid? AssociationTargetTableId { get; set; }

    public MetaTableEntity? AssociationTargetTable { get; set; }

    public bool IsRequired { get; set; }

    public int SortOrder { get; set; }
}
