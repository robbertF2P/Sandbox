namespace Identity.Application.Models;

public sealed record LoginRequest(
    string UserName,
    string Password,
    bool RememberMe);
