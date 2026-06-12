using System.Text.Json.Serialization;
using ApiImportActorPoc.Contracts.Values.Json;

namespace ApiImportActorPoc.Contracts.Values;

[JsonConverter(typeof(LagDaysJsonConverter))]
public readonly struct LagDays : IEquatable<LagDays>, IComparable<LagDays>
{
    public int Value { get; }

    public static LagDays Zero => new(0);

    public LagDays(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Lag cannot be negative.");
        }

        Value = value;
    }

    public static LagDays From(int value) => new(value);

    public static LagDays FromDatabase(int storedValue) => new(Math.Max(0, storedValue));

    public int CompareTo(LagDays other) => Value.CompareTo(other.Value);

    public bool Equals(LagDays other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is LagDays other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value == 0 ? "0d lag" : $"+{Value}d lag";

    public static bool operator ==(LagDays left, LagDays right) => left.Equals(right);

    public static bool operator !=(LagDays left, LagDays right) => !left.Equals(right);
}
