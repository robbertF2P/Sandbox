using Akka.Actor;
using Akka.Hosting;
using AkkaAspirePoc.Api.Actors;
using AkkaAspirePoc.Api.Actors.Messages;
using AkkaAspirePoc.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AkkaAspirePoc.Tests.Actors;

public class TodoActorShould : IAsyncDisposable
{
    private readonly IHost _host;
    private readonly IActorRef _actor;

    public TodoActorShould()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddLogging();
        builder.Services.AddDbContext<TodoDbContext>(options =>
            options.UseSqlite(connection));

        builder.Services.AddAkka("TodoTestSystem", akkaBuilder =>
        {
            akkaBuilder.WithActors((system, registry, resolver) =>
            {
                var props = resolver.Props<TodoActor>();
                var actor = system.ActorOf(props, "todo-actor");
                registry.Register<TodoActor>(actor);
            });
        });

        _host = builder.Build();
        _host.StartAsync().GetAwaiter().GetResult();
        _actor = _host.Services.GetRequiredService<IRequiredActor<TodoActor>>().ActorRef;

        using var scope = _host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        db.Database.EnsureCreated();
    }

    [Test]
    public async Task CreateTodo_PersistsAndReturnsId()
    {
        var result = await _actor.Ask<CreateTodoResult>(new CreateTodoCommand("Actor test todo"));

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.TodoId).IsNotNull();

        using var scope = _host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var count = await db.TodoItems.CountAsync();

        await Assert.That(count).IsEqualTo(1);
    }

    [Test]
    public async Task GetTodos_ReturnsPersistedItems()
    {
        await _actor.Ask<CreateTodoResult>(new CreateTodoCommand("First"));
        await _actor.Ask<CreateTodoResult>(new CreateTodoCommand("Second"));

        var result = await _actor.Ask<GetTodosResult>(new GetTodosQuery());

        await Assert.That(result.Todos.Count).IsEqualTo(2);
        await Assert.That(result.Todos.Select(t => t.Title)).Contains("First");
        await Assert.That(result.Todos.Select(t => t.Title)).Contains("Second");
    }

    [Test]
    public async Task CompleteTodo_MarksItemCompleted()
    {
        var created = await _actor.Ask<CreateTodoResult>(new CreateTodoCommand("Finish this"));
        var completed = await _actor.Ask<CompleteTodoResult>(new CompleteTodoCommand(created.TodoId!.Value));

        await Assert.That(completed.Success).IsTrue();

        using var scope = _host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var entity = await db.TodoItems.SingleAsync();

        await Assert.That(entity.IsCompleted).IsTrue();
        await Assert.That(entity.CompletedAtUtc).IsNotNull();
    }

    public async ValueTask DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();
    }
}
