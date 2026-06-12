namespace ApiImportActorPoc.Contracts.Messages.Import;

public sealed record PersistImportCommand(Guid SessionId) : IActorSystemMessage;
