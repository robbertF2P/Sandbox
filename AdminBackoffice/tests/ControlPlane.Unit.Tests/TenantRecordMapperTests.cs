using ControlPlane.Application.Services;
using Platform.ControlPlane.Contracts;

namespace ControlPlane.Unit.Tests;

public sealed class TenantRecordMapperTests
{
    [Fact]
    public void BuildDeploymentProfile_ForNativeMode_RequiresNativeFields()
    {
        var request = new ProvisionTenantRequest(
            "acme",
            "Acme Shipyard",
            TenantDeploymentMode.Native,
            TenantDataTier.SharedSqlServer,
            "eu-west",
            null,
            null,
            null,
            "vault:tenants/acme/native-db",
            "http://localhost:5080",
            ["sap-v1"],
            ["acme-rules-v1"]);

        var profile = TenantRecordMapper.BuildDeploymentProfile(request);

        Assert.Equal(TenantDeploymentMode.Native, profile.Mode);
        Assert.NotNull(profile.Native);
        Assert.Equal("vault:tenants/acme/native-db", profile.Native.DatabaseConnectionRef);
    }

    [Fact]
    public void BuildDeploymentProfile_ForLegacyMode_RequiresLegacyFields()
    {
        var request = new ProvisionTenantRequest(
            "legacy-acme",
            "Legacy Acme",
            TenantDeploymentMode.LegacyHosted,
            TenantDataTier.DedicatedSqlServer,
            "eu-west",
            "acme-onprem-2024",
            "https://legacy.acme.internal",
            "vault:tenants/legacy-acme/legacy-db",
            null,
            null,
            [],
            []);

        var profile = TenantRecordMapper.BuildDeploymentProfile(request);

        Assert.Equal(TenantDeploymentMode.LegacyHosted, profile.Mode);
        Assert.NotNull(profile.Legacy);
        Assert.Equal("acme-onprem-2024", profile.Legacy.BuildProfileId);
    }

    [Fact]
    public void WithSyncResult_OnSuccess_SetsActiveStatus()
    {
        var tenant = CreateTenant(TenantLifecycleStatus.Provisioning);
        var syncedAt = DateTimeOffset.Parse("2026-06-24T12:00:00Z");

        var updated = TenantRecordMapper.WithSyncResult(tenant, success: true, errorMessage: null, syncedAt);

        Assert.Equal(TenantLifecycleStatus.Active, updated.Status);
        Assert.Equal(syncedAt, updated.ProvisionedAt);
        Assert.Equal(syncedAt, updated.LastSyncedToPlatformAt);
        Assert.Null(updated.LastPlatformSyncError);
    }

    private static TenantRecord CreateTenant(TenantLifecycleStatus status) =>
        new(
            Guid.Parse("3f2e9b1a-8c4d-4e5f-9a0b-1c2d3e4f5a6b"),
            "acme",
            "Acme",
            status,
            TenantDeploymentProfile.CreateNative(
                TenantDataTier.SharedSqlServer,
                "eu-west",
                new NativeRuntimeProfile("vault:db", "http://localhost:5080")),
            new TenantPackEntitlements([], []),
            new TenantMigrationState(TenantMigrationPhase.None, null, null, null),
            new TenantBillingStub("standard", 50),
            DateTimeOffset.UtcNow,
            null,
            null,
            null);
}
