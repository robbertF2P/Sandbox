namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record CreateProjectCommand(
    Guid OrganisationId,
    string Name,
    string? Description) : IActorSystemMessage;
