using System.Net;
using System.Net.Http.Json;

namespace Reference.Characterization.Tests;

[Trait("Module", "Reference")]
[Trait("Tier", "Characterization")]
public sealed class ReferenceStatusEndpointTests(F2pPlatformWebApplicationFactory factory) : IClassFixture<F2pPlatformWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetReferenceStatus_ReturnsHealthyModuleSnapshot()
    {
        using HttpResponseMessage response = await _client.GetAsync("/api/reference/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ReferenceStatusResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Reference", payload.ModuleName);
        Assert.Equal("Healthy", payload.Health);
        Assert.True(payload.ModuleRegistered);
        Assert.False(payload.StranglerAdapterPresent);
    }

    private sealed record ReferenceStatusResponse(
        string ModuleName,
        string Health,
        bool ModuleRegistered,
        bool StranglerAdapterPresent,
        DateTimeOffset CheckedAtUtc);
}
