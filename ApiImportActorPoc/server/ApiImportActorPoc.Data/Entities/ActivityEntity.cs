namespace ApiImportActorPoc.Data.Entities;

public sealed class ActivityEntity
{
    public Guid Id { get; set; }

    public Guid ComponentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ComponentEntity Component { get; set; } = null!;

    public ICollection<AssignmentEntity> Assignments { get; set; } = [];

    public ICollection<ActivityRelationEntity> OutgoingRelations { get; set; } = [];
}
