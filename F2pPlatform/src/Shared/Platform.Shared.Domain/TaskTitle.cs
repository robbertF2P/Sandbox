namespace Platform.Shared.Domain;

public readonly record struct TaskTitle
{
    public string Value { get; }

    public TaskTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Task title is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
