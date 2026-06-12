using ApiImportActorPoc.Contracts.Values;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ApiImportActorPoc.Data.Conversions;

public static class ValueTypeConversions
{
    public static PropertyBuilder<Hours> HasHoursColumn(this PropertyBuilder<Hours> builder) =>
        builder.HasConversion(HoursConverter.Instance).HasPrecision(18, 2);

    public static PropertyBuilder<DurationDays> HasDurationDaysColumn(this PropertyBuilder<DurationDays> builder) =>
        builder.HasConversion(DurationDaysConverter.Instance).HasPrecision(18, 2);

    public static PropertyBuilder<LagDays> HasLagDaysColumn(this PropertyBuilder<LagDays> builder) =>
        builder.HasConversion(LagDaysConverter.Instance);

    public static PropertyBuilder<PersonName> HasPersonNameColumn(this PropertyBuilder<PersonName> builder) =>
        builder.HasConversion(PersonNameConverter.Instance).HasMaxLength(256);

    public static PropertyBuilder<ScheduleDate> HasScheduleDateColumn(this PropertyBuilder<ScheduleDate> builder) =>
        builder.HasConversion(ScheduleDateConverter.Instance);

    private sealed class HoursConverter : ValueConverter<Hours, decimal>
    {
        public static HoursConverter Instance { get; } = new();

        public HoursConverter()
            : base(hours => hours.Value, stored => Hours.FromDatabase(stored))
        {
        }
    }

    private sealed class DurationDaysConverter : ValueConverter<DurationDays, decimal>
    {
        public static DurationDaysConverter Instance { get; } = new();

        public DurationDaysConverter()
            : base(duration => duration.Value, stored => DurationDays.FromDatabase(stored))
        {
        }
    }

    private sealed class LagDaysConverter : ValueConverter<LagDays, int>
    {
        public static LagDaysConverter Instance { get; } = new();

        public LagDaysConverter()
            : base(lag => lag.Value, stored => LagDays.FromDatabase(stored))
        {
        }
    }

    private sealed class PersonNameConverter : ValueConverter<PersonName, string>
    {
        public static PersonNameConverter Instance { get; } = new();

        public PersonNameConverter()
            : base(person => person.Value, stored => PersonName.FromDatabase(stored))
        {
        }
    }

    private sealed class ScheduleDateConverter : ValueConverter<ScheduleDate, DateOnly>
    {
        public static ScheduleDateConverter Instance { get; } = new();

        public ScheduleDateConverter()
            : base(date => date.Value, stored => ScheduleDate.FromDatabase(stored))
        {
        }
    }
}
