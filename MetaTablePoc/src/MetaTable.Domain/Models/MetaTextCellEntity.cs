namespace MetaTable.Domain.Models;

public sealed class MetaTextCellEntity
{
    public Guid RowId { get; set; }

    public MetaRowEntity Row { get; set; } = null!;

    public Guid ColumnId { get; set; }

    public MetaColumnEntity Column { get; set; } = null!;

    public required string Value { get; set; }
}
