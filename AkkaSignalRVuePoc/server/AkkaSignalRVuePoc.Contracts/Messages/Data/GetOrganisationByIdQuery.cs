namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record GetOrganisationByIdQuery(Guid Id) : IActorSystemMessage;
