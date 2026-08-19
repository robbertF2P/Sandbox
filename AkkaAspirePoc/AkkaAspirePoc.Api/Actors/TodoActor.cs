using Akka.Actor;
using AkkaAspirePoc.Api.Actors.Messages;
using AkkaAspirePoc.Data;
using AkkaAspirePoc.Domain.Entities;
using AkkaAspirePoc.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace AkkaAspirePoc.Api.Actors;

public sealed class TodoActor : ReceiveActor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TodoActor> _logger;

    public TodoActor(IServiceScopeFactory scopeFactory, ILogger<TodoActor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        ReceiveAsync<CreateTodoCommand>(HandleCreate);
        ReceiveAsync<GetTodosQuery>(HandleGetAll);
        ReceiveAsync<CompleteTodoCommand>(HandleComplete);
    }

    private async Task HandleCreate(CreateTodoCommand command)
    {
        if (!TodoRules.IsValidTitle(command.Title))
        {
            Sender.Tell(new CreateTodoResult(false, null, "Title is required and must be 200 characters or fewer."));
            return;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        var todo = TodoItem.Create(command.Title, DateTime.UtcNow);
        db.TodoItems.Add(TodoMapper.ToEntity(todo));
        await db.SaveChangesAsync();

        _logger.LogInformation("Created todo {TodoId} via actor", todo.Id);
        Sender.Tell(new CreateTodoResult(true, todo.Id, null));
    }

    private async Task HandleGetAll(GetTodosQuery _)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        var todos = await db.TodoItems
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAtUtc)
            .Select(t => new TodoDto(t.Id, t.Title, t.IsCompleted, t.CreatedAtUtc, t.CompletedAtUtc))
            .ToListAsync();

        Sender.Tell(new GetTodosResult(todos));
    }

    private async Task HandleComplete(CompleteTodoCommand command)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        var entity = await db.TodoItems.FindAsync(command.TodoId);
        if (entity is null)
        {
            Sender.Tell(new CompleteTodoResult(false, "Todo not found."));
            return;
        }

        if (!entity.IsCompleted)
        {
            entity.IsCompleted = true;
            entity.CompletedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();
            _logger.LogInformation("Completed todo {TodoId} via actor", command.TodoId);
        }

        Sender.Tell(new CompleteTodoResult(true, null));
    }
}
