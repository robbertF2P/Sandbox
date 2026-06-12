using ApiImportActorPoc.Contracts.Values;
using ApiImportActorPoc.Data.Entities;

namespace ApiImportActorPoc.Data.Planning.Entities;

public sealed class AssignmentPlanEntity
{
    public int AssignmentId { get; set; }

    public DurationDays DurationDays { get; set; }

    public AssignmentEntity Assignment { get; set; } = null!;
}
