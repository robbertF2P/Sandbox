namespace AkkaAspirePoc.Data.Entities;

public sealed class TodoItemEntity
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }
}
