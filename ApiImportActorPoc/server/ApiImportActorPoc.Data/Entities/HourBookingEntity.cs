namespace ApiImportActorPoc.Data.Entities;

public sealed class HourBookingEntity
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }

    public decimal Hours { get; set; }

    public DateTimeOffset BookedAt { get; set; }

    public string? Notes { get; set; }

    public AssignmentEntity Assignment { get; set; } = null!;
}
