namespace EfCoreHoursNormalization.Values;

/// <summary>
/// Hours stored as SQL Server <c>real</c>. Values within <see cref="Epsilon"/> of zero
/// are normalized to true zero on construction and when read from the database.
/// </summary>
public readonly struct Hours : IEquatable<Hours>, IComparable<Hours>
{
    /// <summary>
    /// Values with absolute magnitude below this threshold are treated as zero.
    /// Tuned for <c>real</c> (float32) columns polluted by floating-point residue.
    /// </summary>
    public const float Epsilon = 1e-6f;

    public static Hours Zero => new(0f);

    public float Value { get; }

    private Hours(float value)
    {
        Value = Normalize(value);
    }

    public static Hours FromHours(float hours) => new(hours);

    public static Hours FromDatabase(float storedValue) => new(storedValue);

    public static float Normalize(float value) =>
        MathF.Abs(value) < Epsilon ? 0f : value;

    public bool IsZero => Value == 0f;

    public int CompareTo(Hours other) => Value.CompareTo(other.Value);

    public bool Equals(Hours other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is Hours other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString("G9");

    public static bool operator ==(Hours left, Hours right) => left.Equals(right);

    public static bool operator !=(Hours left, Hours right) => !left.Equals(right);

    public static Hours operator +(Hours left, Hours right) => FromHours(left.Value + right.Value);

    public static Hours operator -(Hours left, Hours right) => FromHours(left.Value - right.Value);
}
