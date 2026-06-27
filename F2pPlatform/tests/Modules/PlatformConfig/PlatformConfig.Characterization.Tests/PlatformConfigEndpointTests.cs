using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.ControlPlane.Contracts;

namespace PlatformConfig.Characterization.Tests;

public sealed class PlatformConfigWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _sqlitePath = Path.Combine(
        Path.GetTempPath(),
        $"hour-approvals-{Guid.NewGuid():N}.db");
    private readonly string _platformConfigSqlitePath = Path.Combine(
        Path.GetTempPath(),
        $"platform-config-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Platform:ConfigurationApiKey", "test-platform-config-key");
        builder.UseSetting("Tenant:FeatureFlags:hours-progress-approval", "true");
        builder.UseSetting("Tenant:PackEntitlements:customizationPacks:0", "acme-hour-approvals-v1");
        builder.UseSetting("HourApprovals:SqlitePath", _sqlitePath);
        builder.UseSetting("PlatformConfig:SqlitePath", _platformConfigSqlitePath);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (File.Exists(_sqlitePath))
            {
                File.Delete(_sqlitePath);
            }

            if (File.Exists(_platformConfigSqlitePath))
            {
                File.Delete(_platformConfigSqlitePath);
            }
        }

        base.Dispose(disposing);
    }
}

[Trait("Module", "PlatformConfig")]
[Trait("Tier", "Characterization")]
public sealed class PlatformConfigEndpointTests(PlatformConfigWebApplicationFactory factory)
    : IClassFixture<PlatformConfigWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RegisterTenantConfiguration_PersistsAndLists()
    {
        var tenantId = Guid.NewGuid();
        var payload = CreatePayload(tenantId, "acme-shipyard");

        using HttpRequestMessage registerRequest = CreateAuthorizedRequest(
            HttpMethod.Put,
            "/api/v1/platform/tenant-config",
            payload);
        using HttpResponseMessage registerResponse = await _client.SendAsync(registerRequest);
        using HttpResponseMessage listResponse = await _client.GetAsync("/api/v1/platform/tenants");

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var tenants = await listResponse.Content.ReadFromJsonAsync<List<TenantConfigurationPayload>>();
        Assert.NotNull(tenants);
        Assert.Contains(tenants, tenant => tenant.Slug == "acme-shipyard");
    }

    [Fact]
    public async Task RegisterTenantConfiguration_RequiresApiKey()
    {
        var payload = CreatePayload(Guid.NewGuid(), "missing-key");

        using HttpResponseMessage response = await _client.PutAsJsonAsync(
            "/api/v1/platform/tenant-config",
            payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static TenantConfigurationPayload CreatePayload(Guid tenantId, string slug) =>
        new(
            tenantId,
            slug,
            "Acme Shipyard",
            TenantLifecycleStatus.Active,
            TenantDeploymentProfile.CreateNative(
                TenantDataTier.SharedSqlServer,
                "eu-west",
                new NativeRuntimeProfile("vault:tenants/acme/native-db", "https://api.example/v1")),
            new TenantPackEntitlements([], ["default-hour-approvals-v1"]),
            new TenantMigrationState(TenantMigrationPhase.None, null, null, null),
            new TenantBillingStub("standard", 50));

    private static HttpRequestMessage CreateAuthorizedRequest(
        HttpMethod method,
        string path,
        TenantConfigurationPayload payload)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(payload),
        };
        request.Headers.Add("X-Platform-Config-Key", "test-platform-config-key");
        return request;
    }
}
