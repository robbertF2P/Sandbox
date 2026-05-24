namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record GetProjectByIdQuery(Guid Id) : IActorSystemMessage;
