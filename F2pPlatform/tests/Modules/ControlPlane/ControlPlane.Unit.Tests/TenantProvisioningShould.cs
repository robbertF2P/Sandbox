using ControlPlane.Application.Ports;
using ControlPlane.Application.Tenants;
using ControlPlane.Domain.Tenants;

namespace ControlPlane.Unit.Tests;

[Trait("Module", "ControlPlane")]
public sealed class TenantProvisioningRulesShould
{
    [Theory]
    [InlineData("acme-shipyard", true)]
    [InlineData("acme", true)]
    [InlineData("a1-b2-c3", true)]
    [InlineData("Acme", false)]
    [InlineData("acme_shipyard", false)]
    [InlineData("", false)]
    public void IsValidSlug_MatchesExpectedPattern(string slug, bool expected) =>
        Assert.Equal(expected, TenantProvisioningRules.IsValidSlug(slug));

    [Fact]
    public void CreateNativeTenant_ProducesProvisioningRecord()
    {
        DateTimeOffset createdAt = new(2026, 6, 24, 12, 0, 0, TimeSpan.Zero);

        TenantRecord tenant = TenantProvisioningRules.CreateNativeTenant(
            "acme-shipyard",
            "Acme Shipyard",
            TenantDataTier.SharedSqlServer,
            "eu-west",
            "vault:tenants/acme-shipyard/native-db",
            "https://api.platform.example/v1",
            "enterprise",
            50,
            createdAt);

        Assert.NotEqual(Guid.Empty, tenant.TenantId);
        Assert.Equal("acme-shipyard", tenant.Slug);
        Assert.Equal("Acme Shipyard", tenant.DisplayName);
        Assert.Equal(TenantLifecycleStatus.Provisioning, tenant.Status);
        Assert.Equal(TenantDeploymentMode.Native, tenant.DeploymentProfile.Mode);
        Assert.Equal("eu-west", tenant.DeploymentProfile.Region);
        Assert.Equal("enterprise", tenant.Billing.Tier);
        Assert.Equal(50, tenant.Billing.SeatLimit);
        Assert.Equal(createdAt, tenant.CreatedAtUtc);
    }
}

public sealed class TenantProvisioningServiceShould
{
    [Fact]
    public async Task CreateTenantAsync_RejectsDuplicateSlug()
    {
        var repository = new FakeTenantRepository();
        var service = new TenantProvisioningService(repository);
        var request = new CreateTenantRequest(
            "acme-shipyard",
            "Acme Shipyard",
            "shared_sql_server",
            "eu-west",
            "vault:tenants/acme-shipyard/native-db",
            null,
            "enterprise",
            50);

        await service.CreateTenantAsync(request, CancellationToken.None);

        await Assert.ThrowsAsync<TenantSlugConflictException>(() =>
            service.CreateTenantAsync(request, CancellationToken.None));
    }

    private sealed class FakeTenantRepository : ITenantRepository
    {
        private readonly List<TenantRecord> _tenants = [];

        public Task<IReadOnlyList<TenantRecord>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TenantRecord>>(_tenants);

        public Task<TenantRecord?> GetBySlugAsync(string slug, CancellationToken cancellationToken) =>
            Task.FromResult(_tenants.FirstOrDefault(tenant => tenant.Slug == slug));

        public Task AddAsync(TenantRecord tenant, CancellationToken cancellationToken)
        {
            _tenants.Add(tenant);
            return Task.CompletedTask;
        }
    }
}
