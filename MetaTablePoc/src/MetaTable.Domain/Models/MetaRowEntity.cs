namespace MetaTable.Domain.Models;

public sealed class MetaRowEntity
{
    public Guid Id { get; set; }

    public Guid TableId { get; set; }

    public MetaTableEntity Table { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}
