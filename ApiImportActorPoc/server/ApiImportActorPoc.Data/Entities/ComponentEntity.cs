namespace ApiImportActorPoc.Data.Entities;

public sealed class ComponentEntity
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Guid? ParentComponentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ProjectEntity Project { get; set; } = null!;

    public ComponentEntity? ParentComponent { get; set; }

    public ICollection<ComponentEntity> ChildComponents { get; set; } = [];

    public ICollection<ActivityEntity> Activities { get; set; } = [];
}
