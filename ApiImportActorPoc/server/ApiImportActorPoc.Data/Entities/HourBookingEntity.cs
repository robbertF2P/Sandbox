using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Data.Entities;

public sealed class HourBookingEntity
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }

    public Hours Hours { get; set; } = Hours.Zero;

    public DateTimeOffset BookedAt { get; set; }

    public string? Notes { get; set; }

    public AssignmentEntity Assignment { get; set; } = null!;
}
