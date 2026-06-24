using Identity.Application.Models;
using Identity.Infrastructure.Services;

namespace Identity.Unit.Tests;

[Trait("Module", "Identity")]
public sealed class DummyIdentityLoginServiceTests
{
  private readonly DummyIdentityLoginService _service = new();

  [Fact]
  public async Task LoginAsync_AcceptsAnyCredentials_ReturnsSession()
  {
    LoginResponse response = await _service.LoginAsync(
      new LoginRequest("reg user", "test", RememberMe: false),
      CancellationToken.None);

    Assert.Equal("reg user", response.UserName);
    Assert.Equal("reg user", response.DisplayName);
    Assert.False(string.IsNullOrWhiteSpace(response.Token));
    Assert.True(response.ExpiresAtUtc > DateTimeOffset.UtcNow);
  }

  [Fact]
  public async Task LoginAsync_WithRememberMe_ExtendsExpiry()
  {
    LoginResponse shortLived = await _service.LoginAsync(
      new LoginRequest("user", "pwd", RememberMe: false),
      CancellationToken.None);
    LoginResponse remembered = await _service.LoginAsync(
      new LoginRequest("user", "pwd", RememberMe: true),
      CancellationToken.None);

    Assert.True(remembered.ExpiresAtUtc > shortLived.ExpiresAtUtc);
  }
}
