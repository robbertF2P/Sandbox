using Identity.Application.Models;
using Identity.Application.Ports;

namespace Identity.Infrastructure.Services;

internal static class PocUserProfiles
{
    public const string ApproveHoursProgress = "ApproveHoursProgress";

    public static IReadOnlyList<string> ResolvePermissions(string userName)
    {
        if (userName.Contains("supervisor", StringComparison.OrdinalIgnoreCase))
        {
            return [ApproveHoursProgress];
        }

        return [];
    }
}

/// <summary>
/// POC login — accepts any credentials and issues a short-lived session token.
/// Replace with OIDC / user store per platform-authentication-standard.md.
/// </summary>
public sealed class DummyIdentityLoginService : IIdentityLoginService
{
    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string userName = string.IsNullOrWhiteSpace(request.UserName)
            ? "guest"
            : request.UserName.Trim();

        TimeSpan lifetime = request.RememberMe
            ? TimeSpan.FromDays(30)
            : TimeSpan.FromHours(8);

        DateTimeOffset expiresAtUtc = DateTimeOffset.UtcNow.Add(lifetime);
        string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        return Task.FromResult(new LoginResponse(
            userName,
            userName,
            token,
            expiresAtUtc,
            PocUserProfiles.ResolvePermissions(userName)));
    }
}
