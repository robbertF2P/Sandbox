namespace ImportPipeline.Domain;

/// <summary>
/// Explicit absence — use instead of null for values that may legitimately not exist.
/// Map transforms the inner value; Bind chains steps that may also return None.
/// </summary>
public readonly struct Option<T>
{
    private readonly T? _value;
    private readonly bool _isSome;

    private Option(T value) { _value = value; _isSome = true; }

    public bool IsSome => _isSome;
    public bool IsNone => !_isSome;

    public static Option<T> Some(T value) => new(value);
    public static readonly Option<T> None = default;

    public T Value => _isSome
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on None.");

    public Option<R> Map<R>(Func<T, R> f) =>
        _isSome ? Option<R>.Some(f(_value!)) : Option<R>.None;

    public Option<R> Bind<R>(Func<T, Option<R>> f) =>
        _isSome ? f(_value!) : Option<R>.None;

    public T GetValueOrDefault(T fallback) => _isSome ? _value! : fallback;

    public bool TryGetValue(out T value)
    {
        value = _isSome ? _value! : default!;
        return _isSome;
    }

    public override string ToString() => _isSome ? $"Some({_value})" : "None";
}

public static class Option
{
    public static Option<T> Some<T>(T value) => Option<T>.Some(value);
    public static Option<T> None<T>() => Option<T>.None;

    /// Converts a null-or-empty string to None; any other value becomes Some.
    public static Option<string> FromNullOrEmpty(string? value) =>
        string.IsNullOrEmpty(value) ? Option<string>.None : Option<string>.Some(value);
}
