namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record DeleteProjectCommand(Guid Id) : IActorSystemMessage;
