using EfCoreHoursNormalization.Values;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EfCoreHoursNormalization.Conversions;

public sealed class HoursValueConverter : ValueConverter<Hours, float>
{
    public static HoursValueConverter Instance { get; } = new();

    public HoursValueConverter()
        : base(
            hours => hours.Value,
            stored => Hours.FromDatabase(stored))
    {
    }
}
