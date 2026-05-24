namespace AkkaSignalRVuePoc.Contracts.Data;

public sealed record ProjectDto(
    Guid Id,
    Guid OrganisationId,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt);
