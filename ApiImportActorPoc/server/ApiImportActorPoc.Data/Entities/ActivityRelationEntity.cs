namespace ApiImportActorPoc.Data.Entities;

public sealed class ActivityRelationEntity
{
    public Guid Id { get; set; }

    public Guid SourceActivityId { get; set; }

    public Guid TargetActivityId { get; set; }

    public string RelationType { get; set; } = string.Empty;

    public ActivityEntity SourceActivity { get; set; } = null!;

    public ActivityEntity TargetActivity { get; set; } = null!;
}
