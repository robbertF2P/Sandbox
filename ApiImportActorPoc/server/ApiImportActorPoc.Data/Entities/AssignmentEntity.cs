using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Data.Entities;

public sealed class AssignmentEntity
{
    public int Id { get; set; }

    public int ActivityId { get; set; }

    public PersonName PersonName { get; set; } = PersonName.Open;

    public string? Description { get; set; }

    public Hours BudgetedHours { get; set; } = Hours.Zero;

    public ActivityEntity Activity { get; set; } = null!;

    public ICollection<HourBookingEntity> HourBookings { get; set; } = [];
}
