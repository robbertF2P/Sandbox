using System.Text.Json.Serialization;
using ApiImportActorPoc.Contracts.Values.Json;

namespace ApiImportActorPoc.Contracts.Values;

[JsonConverter(typeof(HoursJsonConverter))]
public readonly struct Hours : IEquatable<Hours>, IComparable<Hours>
{
    public decimal Value { get; }

    public static Hours Zero => new(0m);

    public Hours(decimal value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Hours cannot be negative.");
        }

        Value = value;
    }

    public static Hours From(decimal value) => new(value);

    public static Hours FromDatabase(decimal storedValue) => new(storedValue);

    public bool IsZero => Value == 0m;

    public int CompareTo(Hours other) => Value.CompareTo(other.Value);

    public bool Equals(Hours other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is Hours other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString("0.##");

    public static Hours operator +(Hours left, Hours right) => new(left.Value + right.Value);

    public static bool operator >(Hours left, Hours right) => left.Value > right.Value;

    public static bool operator <(Hours left, Hours right) => left.Value < right.Value;

    public static bool operator <=(Hours left, Hours right) => left.Value <= right.Value;

    public static bool operator >=(Hours left, Hours right) => left.Value >= right.Value;

    public static bool operator ==(Hours left, Hours right) => left.Equals(right);

    public static bool operator !=(Hours left, Hours right) => !left.Equals(right);
}
