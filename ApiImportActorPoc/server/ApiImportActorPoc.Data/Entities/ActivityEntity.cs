namespace ApiImportActorPoc.Data.Entities;

public sealed class ActivityEntity
{
    public int Id { get; set; }

    public int ComponentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ComponentEntity Component { get; set; } = null!;

    public ICollection<AssignmentEntity> Assignments { get; set; } = [];

    public ICollection<ActivityRelationEntity> OutgoingRelations { get; set; } = [];
}
