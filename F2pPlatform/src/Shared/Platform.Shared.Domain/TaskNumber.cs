namespace Platform.Shared.Domain;

public readonly record struct TaskNumber(int Value)
{
    public override string ToString() => Value.ToString();
}
