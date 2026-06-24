using Identity.Application.Models;

namespace Identity.Application.Ports;

public interface IIdentityLoginService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
