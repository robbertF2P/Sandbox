using System.Net;
using System.Net.Http.Json;

namespace Identity.Characterization.Tests;

[Trait("Module", "Identity")]
[Trait("Tier", "Characterization")]
public sealed class IdentityLoginEndpointTests(F2pPlatformWebApplicationFactory factory) : IClassFixture<F2pPlatformWebApplicationFactory>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task PostLogin_AcceptsAnyCredentials_ReturnsSession()
  {
    using HttpResponseMessage response = await _client.PostAsJsonAsync(
      "/api/identity/login",
      new { UserName = "reg user", Password = "test", RememberMe = false });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
    Assert.NotNull(payload);
    Assert.Equal("reg user", payload.UserName);
    Assert.False(string.IsNullOrWhiteSpace(payload.Token));
  }

  private sealed record LoginResponse(
    string UserName,
    string DisplayName,
    string Token,
    DateTimeOffset ExpiresAtUtc);
}
