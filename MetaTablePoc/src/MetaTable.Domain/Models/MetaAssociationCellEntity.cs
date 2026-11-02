namespace MetaTable.Domain.Models;

public sealed class MetaAssociationCellEntity
{
    public Guid RowId { get; set; }

    public MetaRowEntity Row { get; set; } = null!;

    public Guid ColumnId { get; set; }

    public MetaColumnEntity Column { get; set; } = null!;

    public Guid TargetRowId { get; set; }

    public MetaRowEntity TargetRow { get; set; } = null!;
}
