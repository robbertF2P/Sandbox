namespace MetaTable.Domain.Models;

public sealed class MetaTableEntity
{
    public Guid Id { get; set; }

    public required string Key { get; set; }

    public required string DisplayName { get; set; }
}
