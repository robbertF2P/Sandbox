namespace AkkaAspirePoc.Domain.Entities;

public sealed record TodoItem
{
    public Guid Id { get; init; }

    public required string Title { get; init; }

    public bool IsCompleted { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }

    public static TodoItem Create(string title, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        return new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            IsCompleted = false,
            CreatedAtUtc = createdAtUtc
        };
    }

    public TodoItem MarkCompleted(DateTime completedAtUtc) =>
        IsCompleted
            ? this
            : this with
            {
                IsCompleted = true,
                CompletedAtUtc = completedAtUtc
            };
}
