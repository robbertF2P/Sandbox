using System.Text.Json.Serialization;
using ApiImportActorPoc.Contracts.Values.Json;

namespace ApiImportActorPoc.Contracts.Values;

[JsonConverter(typeof(DurationDaysJsonConverter))]
public readonly struct DurationDays : IEquatable<DurationDays>, IComparable<DurationDays>
{
    public decimal Value { get; }

    public DurationDays(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Duration must be greater than zero.");
        }

        Value = value;
    }

    public static DurationDays From(decimal value) => new(value);

    public static DurationDays FromDatabase(decimal storedValue) => storedValue <= 0 ? From(0.5m) : new(storedValue);

    public static DurationDays FromBudgetedHours(Hours budgetedHours, decimal hoursPerDay = 8m)
    {
        var days = budgetedHours.Value / hoursPerDay;
        return From(Math.Max(0.5m, Math.Round(days, 2)));
    }

    public int CompareTo(DurationDays other) => Value.CompareTo(other.Value);

    public bool Equals(DurationDays other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is DurationDays other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => $"{Value}d";

    public static bool operator >(DurationDays left, DurationDays right) => left.Value > right.Value;

    public static bool operator <(DurationDays left, DurationDays right) => left.Value < right.Value;

    public static bool operator ==(DurationDays left, DurationDays right) => left.Equals(right);

    public static bool operator !=(DurationDays left, DurationDays right) => !left.Equals(right);
}
