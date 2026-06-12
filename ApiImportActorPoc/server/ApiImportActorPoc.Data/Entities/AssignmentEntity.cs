namespace ApiImportActorPoc.Data.Entities;

public sealed class AssignmentEntity
{
    public int Id { get; set; }

    public int ActivityId { get; set; }

    public string PersonName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal BudgetedHours { get; set; }

    public ActivityEntity Activity { get; set; } = null!;

    public ICollection<HourBookingEntity> HourBookings { get; set; } = [];
}
