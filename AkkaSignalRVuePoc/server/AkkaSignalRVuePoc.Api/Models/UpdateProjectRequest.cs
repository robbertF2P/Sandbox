namespace AkkaSignalRVuePoc.Api.Models;

public sealed record UpdateProjectRequest(
    string? Name,
    string? Description);
