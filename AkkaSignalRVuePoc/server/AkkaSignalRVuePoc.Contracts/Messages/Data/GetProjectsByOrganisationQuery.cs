namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record GetProjectsByOrganisationQuery(Guid OrganisationId) : IActorSystemMessage;
