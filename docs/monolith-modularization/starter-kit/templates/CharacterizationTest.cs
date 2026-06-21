// Copy into tests/Modules/<Context>/<Context>.Characterization.Tests/
// Replace <Context> and endpoint paths for your module smoke test.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace <Context>.Characterization.Tests;

public sealed class <Context>WebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}

[Trait("Module", "<Context>")]
[Trait("Tier", "Characterization")]
public sealed class <Context>StatusEndpointTests(<Context>WebApplicationFactory factory)
    : IClassFixture<<Context>WebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get<Context>Status_ReturnsOk()
    {
        using HttpResponseMessage response = await _client.GetAsync("/api/<context>/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
