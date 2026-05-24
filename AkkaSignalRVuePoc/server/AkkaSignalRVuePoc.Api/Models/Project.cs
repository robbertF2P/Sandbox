namespace AkkaSignalRVuePoc.Api.Models;

public sealed record Project(
    Guid Id,
    Guid OrganisationId,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt);
