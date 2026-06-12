namespace ApiImportActorPoc.Data.Entities;

public sealed class ProjectEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<ComponentEntity> Components { get; set; } = [];
}
