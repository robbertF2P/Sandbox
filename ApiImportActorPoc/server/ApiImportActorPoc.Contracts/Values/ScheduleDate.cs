using System.Text.Json.Serialization;
using ApiImportActorPoc.Contracts.Values.Json;

namespace ApiImportActorPoc.Contracts.Values;

[JsonConverter(typeof(ScheduleDateJsonConverter))]
public readonly struct ScheduleDate : IEquatable<ScheduleDate>, IComparable<ScheduleDate>
{
    public DateOnly Value { get; }

    public ScheduleDate(DateOnly value) => Value = value;

    public static ScheduleDate From(DateOnly value) => new(value);

    public static ScheduleDate From(string isoDate) => new(DateOnly.Parse(isoDate));

    public static ScheduleDate FromDatabase(DateOnly storedValue) => new(storedValue);

    public ScheduleDate AddDays(int days) => new(Value.AddDays(days));

    public int CompareTo(ScheduleDate other) => Value.CompareTo(other.Value);

    public bool Equals(ScheduleDate other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is ScheduleDate other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString("yyyy-MM-dd");

    public static bool operator >(ScheduleDate left, ScheduleDate right) => left.Value > right.Value;

    public static bool operator <(ScheduleDate left, ScheduleDate right) => left.Value < right.Value;

    public static bool operator ==(ScheduleDate left, ScheduleDate right) => left.Equals(right);

    public static bool operator !=(ScheduleDate left, ScheduleDate right) => !left.Equals(right);
}
