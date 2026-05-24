namespace AkkaSignalRVuePoc.Api.Models;

public sealed record CreateProjectRequest(
    Guid OrganisationId,
    string Name,
    string? Description);
