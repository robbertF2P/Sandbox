using ApiImportActorPoc.Contracts.Values;
using ApiImportActorPoc.Data.Entities;

namespace ApiImportActorPoc.Data.Planning.Entities;

public sealed class ProjectPlanEntity
{
    public int ProjectId { get; set; }

    public ScheduleDate PlannedStartDate { get; set; }

    public DateTimeOffset LastCalculatedAt { get; set; }

    public ProjectEntity Project { get; set; } = null!;
}
