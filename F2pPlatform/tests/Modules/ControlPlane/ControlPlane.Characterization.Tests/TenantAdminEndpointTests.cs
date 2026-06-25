using System.Net;
using System.Net.Http.Json;
using ControlPlane.Application.Tenants;

namespace ControlPlane.Characterization.Tests;

[Trait("Module", "ControlPlane")]
[Trait("Tier", "Characterization")]
public sealed class TenantAdminEndpointTests(F2pPlatformWebApplicationFactory factory) : IClassFixture<F2pPlatformWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ListTenants_ReturnsOkWithTenantCollection()
    {
        using HttpResponseMessage response = await _client.GetAsync("/admin/tenants");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tenants = await response.Content.ReadFromJsonAsync<List<TenantSummaryResponse>>();
        Assert.NotNull(tenants);
    }

    [Fact]
    public async Task CreateTenant_ThenList_IncludesCreatedTenant()
    {
        var slug = $"tenant-{Guid.NewGuid():N}";
        var request = new CreateTenantRequest(
            slug,
            "Listed Tenant",
            "shared_sql_server",
            "eu-west",
            $"vault:tenants/{slug}/native-db",
            "https://api.platform.example/v1",
            "standard",
            10);

        using HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/admin/tenants", request);
        using HttpResponseMessage listResponse = await _client.GetAsync("/admin/tenants");

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var tenants = await listResponse.Content.ReadFromJsonAsync<List<TenantSummaryResponse>>();
        Assert.NotNull(tenants);
        Assert.Contains(tenants, tenant => tenant.Slug == slug);
    }

    [Fact]
    public async Task CreateTenant_ReturnsCreatedTenant()
    {
        var request = new CreateTenantRequest(
            "acme-shipyard",
            "Acme Shipyard",
            "shared_sql_server",
            "eu-west",
            "vault:tenants/acme-shipyard/native-db",
            "https://api.platform.example/v1",
            "enterprise",
            50);

        using HttpResponseMessage response = await _client.PostAsJsonAsync("/admin/tenants", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var tenant = await response.Content.ReadFromJsonAsync<TenantSummaryResponse>();
        Assert.NotNull(tenant);
        Assert.Equal("acme-shipyard", tenant.Slug);
        Assert.Equal("Acme Shipyard", tenant.DisplayName);
        Assert.Equal("provisioning", tenant.Status);
        Assert.Equal("native", tenant.DeploymentMode);
    }

    [Fact]
    public async Task CreateTenant_RejectsDuplicateSlug()
    {
        var request = new CreateTenantRequest(
            "duplicate-tenant",
            "Duplicate Tenant",
            "shared_sql_server",
            "eu-west",
            "vault:tenants/duplicate/native-db",
            null,
            "standard",
            10);

        using HttpResponseMessage first = await _client.PostAsJsonAsync("/admin/tenants", request);
        using HttpResponseMessage second = await _client.PostAsJsonAsync("/admin/tenants", request);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }
}
