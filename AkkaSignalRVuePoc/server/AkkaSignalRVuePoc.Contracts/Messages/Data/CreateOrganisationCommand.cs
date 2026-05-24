namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record CreateOrganisationCommand(string Name) : IActorSystemMessage;
