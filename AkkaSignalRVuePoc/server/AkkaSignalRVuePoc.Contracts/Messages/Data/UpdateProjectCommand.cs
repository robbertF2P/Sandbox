namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record UpdateProjectCommand(
    Guid Id,
    string? Name,
    string? Description) : IActorSystemMessage;
