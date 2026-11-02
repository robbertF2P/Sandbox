namespace MetaTable.Domain.Models;

public sealed class MetaIntegerCellEntity
{
    public Guid RowId { get; set; }

    public MetaRowEntity Row { get; set; } = null!;

    public Guid ColumnId { get; set; }

    public MetaColumnEntity Column { get; set; } = null!;

    public int Value { get; set; }
}
