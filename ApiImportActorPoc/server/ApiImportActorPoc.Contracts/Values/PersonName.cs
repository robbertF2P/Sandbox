using System.Text.Json.Serialization;
using ApiImportActorPoc.Contracts.Values.Json;

namespace ApiImportActorPoc.Contracts.Values;

[JsonConverter(typeof(PersonNameJsonConverter))]
public readonly struct PersonName : IEquatable<PersonName>
{
    public string Value { get; }

    public static PersonName Open => new(string.Empty, skipValidation: true);

    public PersonName(string value)
        : this(value, skipValidation: false)
    {
    }

    private PersonName(string value, bool skipValidation)
    {
        Value = skipValidation ? value : (value ?? string.Empty).Trim();
    }

    public static PersonName From(string? value) => new(value ?? string.Empty);

    public static PersonName FromDatabase(string storedValue) => new(storedValue ?? string.Empty, skipValidation: true);

    public bool IsOpen => string.IsNullOrEmpty(Value);

    public string DisplayLabel => IsOpen ? "Open" : Value;

    public bool Equals(PersonName other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is PersonName other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => DisplayLabel;

    public static bool operator ==(PersonName left, PersonName right) => left.Equals(right);

    public static bool operator !=(PersonName left, PersonName right) => !left.Equals(right);
}
