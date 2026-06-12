namespace ApiImportActorPoc.Data.Entities;

public sealed class ActivityRelationEntity
{
    public int Id { get; set; }

    public int SourceActivityId { get; set; }

    public int TargetActivityId { get; set; }

    public string RelationType { get; set; } = string.Empty;

    public ActivityEntity SourceActivity { get; set; } = null!;

    public ActivityEntity TargetActivity { get; set; } = null!;
}
