namespace AkkaSignalRVuePoc.Api.Models;

public sealed record Organisation(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt);
