namespace Identity.Application.Models;

public sealed record LoginResponse(
    string UserName,
    string DisplayName,
    string Token,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<string> Permissions);
