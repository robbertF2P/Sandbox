using EfCoreHoursNormalization.Values;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EfCoreHoursNormalization.Conversions;

public sealed class HoursValueComparer : ValueComparer<Hours>
{
    public static HoursValueComparer Instance { get; } = new();

    public HoursValueComparer()
        : base(
            (left, right) => left.Equals(right),
            hours => hours.GetHashCode(),
            hours => hours)
    {
    }
}
