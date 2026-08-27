using AkkaAspirePoc.Data.Entities;
using DomainTodo = AkkaAspirePoc.Domain.Entities.TodoItem;

namespace AkkaAspirePoc.Data;

public static class TodoMapper
{
    public static DomainTodo ToDomain(TodoItemEntity entity) =>
        new()
        {
            Id = entity.Id,
            Title = entity.Title,
            IsCompleted = entity.IsCompleted,
            CreatedAtUtc = entity.CreatedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc
        };

    public static TodoItemEntity ToEntity(DomainTodo todo) =>
        new()
        {
            Id = todo.Id,
            Title = todo.Title,
            IsCompleted = todo.IsCompleted,
            CreatedAtUtc = todo.CreatedAtUtc,
            CompletedAtUtc = todo.CompletedAtUtc
        };
}
