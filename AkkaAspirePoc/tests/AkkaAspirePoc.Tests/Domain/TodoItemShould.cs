using AkkaAspirePoc.Domain.Entities;

namespace AkkaAspirePoc.Tests.Domain;

public class TodoItemShould
{
    [Test]
    public async Task Create_AssignsIdAndDefaults()
    {
        var createdAt = new DateTime(2026, 8, 19, 12, 0, 0, DateTimeKind.Utc);

        var todo = TodoItem.Create("Write TUnit tests", createdAt);

        await Assert.That(todo.Id).IsNotDefault();
        await Assert.That(todo.Title).IsEqualTo("Write TUnit tests");
        await Assert.That(todo.IsCompleted).IsFalse();
        await Assert.That(todo.CreatedAtUtc).IsEqualTo(createdAt);
        await Assert.That(todo.CompletedAtUtc).IsNull();
    }

    [Test]
    public async Task Create_TrimsTitle()
    {
        var todo = TodoItem.Create("  trim me  ", DateTime.UtcNow);

        await Assert.That(todo.Title).IsEqualTo("trim me");
    }

    [Test]
    public async Task MarkCompleted_SetsCompletionFields()
    {
        var todo = TodoItem.Create("Complete me", DateTime.UtcNow);
        var completedAt = new DateTime(2026, 8, 19, 13, 0, 0, DateTimeKind.Utc);

        var completed = todo.MarkCompleted(completedAt);

        await Assert.That(completed.IsCompleted).IsTrue();
        await Assert.That(completed.CompletedAtUtc).IsEqualTo(completedAt);
    }

    [Test]
    public async Task MarkCompleted_IsIdempotent()
    {
        var completedAt = new DateTime(2026, 8, 19, 13, 0, 0, DateTimeKind.Utc);
        var todo = TodoItem.Create("Already done", DateTime.UtcNow).MarkCompleted(completedAt);

        var again = todo.MarkCompleted(DateTime.UtcNow.AddHours(1));

        await Assert.That(again).IsEqualTo(todo);
    }
}
