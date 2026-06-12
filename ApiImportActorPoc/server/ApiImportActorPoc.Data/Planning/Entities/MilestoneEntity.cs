using ApiImportActorPoc.Data.Entities;

namespace ApiImportActorPoc.Data.Planning.Entities;

public sealed class MilestoneEntity
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly TargetDate { get; set; }

    public int? ActivityId { get; set; }

    public ProjectEntity Project { get; set; } = null!;

    public ActivityEntity? Activity { get; set; }
}
