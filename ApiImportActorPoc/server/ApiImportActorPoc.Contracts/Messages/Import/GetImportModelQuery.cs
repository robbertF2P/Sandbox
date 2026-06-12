namespace ApiImportActorPoc.Contracts.Messages.Import;

public sealed record GetImportModelQuery(Guid SessionId) : IActorSystemMessage;
