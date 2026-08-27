using AkkaAspirePoc.Data;
using AkkaAspirePoc.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AkkaAspirePoc.Tests.Data;

public class TodoDbContextShould : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TodoDbContext _db;

    public TodoDbContextShould()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new TodoDbContext(options);
        _db.Database.EnsureCreated();
    }

    [Test]
    public async Task CanPersistAndQueryTodoItems()
    {
        _db.TodoItems.Add(new TodoItemEntity
        {
            Id = Guid.NewGuid(),
            Title = "EF Core test",
            IsCompleted = false,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        var items = await _db.TodoItems.AsNoTracking().ToListAsync();

        await Assert.That(items.Count).IsEqualTo(1);
        await Assert.That(items[0].Title).IsEqualTo("EF Core test");
    }

    public async ValueTask DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
