namespace AkkaAspirePoc.Api.Actors.Messages;

public interface ITodoActorMessage;

public sealed record CreateTodoCommand(string Title) : ITodoActorMessage;

public sealed record CreateTodoResult(bool Success, Guid? TodoId, string? Error);

public sealed record GetTodosQuery : ITodoActorMessage;

public sealed record GetTodosResult(IReadOnlyList<TodoDto> Todos);

public sealed record CompleteTodoCommand(Guid TodoId) : ITodoActorMessage;

public sealed record CompleteTodoResult(bool Success, string? Error);

public sealed record TodoDto(Guid Id, string Title, bool IsCompleted, DateTime CreatedAtUtc, DateTime? CompletedAtUtc);
