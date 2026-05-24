namespace AkkaSignalRVuePoc.Contracts.Data;

public sealed record OrganisationDto(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt);
