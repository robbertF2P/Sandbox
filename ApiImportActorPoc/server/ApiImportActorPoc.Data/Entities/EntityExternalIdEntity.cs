using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Data.Entities;

public sealed class EntityExternalIdEntity
{
    public int Id { get; set; }

    public string System { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public ImportEntityKind EntityKind { get; set; }

    public int InternalEntityId { get; set; }
}
