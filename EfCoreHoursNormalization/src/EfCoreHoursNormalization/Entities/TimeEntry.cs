using EfCoreHoursNormalization.Values;

namespace EfCoreHoursNormalization.Entities;

public sealed class TimeEntry
{
    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public Hours Hours { get; set; } = Hours.Zero;
}
