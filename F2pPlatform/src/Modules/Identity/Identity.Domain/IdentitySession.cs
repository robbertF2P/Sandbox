namespace Identity.Domain;

public sealed record IdentitySession(
    string UserName,
    string DisplayName,
    string Token,
    DateTimeOffset ExpiresAtUtc);
