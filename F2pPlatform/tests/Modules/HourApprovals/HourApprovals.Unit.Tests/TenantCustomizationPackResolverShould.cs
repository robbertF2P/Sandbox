using HourApprovals.Application.Ports;
using HourApprovals.Infrastructure;
using HourApprovals.Infrastructure.Packs;
using HourApprovals.Packs.Acme;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.ControlPlane.Contracts;
using PlatformConfig.Application.Ports;
using PlatformConfig.Infrastructure.Runtime;

namespace HourApprovals.Unit.Tests;

[Trait("Module", "HourApprovals")]
public sealed class TenantCustomizationPackResolverShould
{
    [Fact]
    public void ResolveActivePack_UsesTenantRuntimeEntitlements()
    {
        IHourApprovalsCustomizationPack resolver = BuildResolver(
            runtimeContext =>
            {
                runtimeContext.SetCurrent(CreatePayload(["acme-hour-approvals-v1"]));
            },
            new ConfigurationBuilder().Build());

        Assert.Equal("acme-hour-approvals-v1", resolver.PackId);
        Assert.True(resolver.GetView(HourApprovalsScreens.Queue).IsVisible("sapCostElement"));
    }

    [Fact]
    public void ResolveActivePack_FallsBackToConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tenant:PackEntitlements:customizationPacks:0"] = "default-hour-approvals-v1",
            })
            .Build();

        IHourApprovalsCustomizationPack resolver = BuildResolver(_ => { }, configuration);

        Assert.Equal("default-hour-approvals-v1", resolver.PackId);
        Assert.False(resolver.GetView(HourApprovalsScreens.Queue).IsVisible("plannedStart"));
    }

    private static IHourApprovalsCustomizationPack BuildResolver(
        Action<ITenantRuntimeContext> configureRuntime,
        IConfiguration configuration)
    {
        var services = new ServiceCollection();
        var runtimeContext = new TenantRuntimeContext();
        configureRuntime(runtimeContext);

        services.AddSingleton<ITenantRuntimeContext>(runtimeContext);
        services.AddSingleton<IConfiguration>(configuration);
        services.AddHourApprovalsCustomizationPacks(packs =>
        {
            packs.AddPack<DefaultHourApprovalsPack>();
            packs.AddPack<AcmeHourApprovalsPack>();
        });

        return services.BuildServiceProvider().GetRequiredService<IHourApprovalsCustomizationPack>();
    }

    private static TenantConfigurationPayload CreatePayload(string[] customizationPacks) =>
        new(
            Guid.NewGuid(),
            "acme-shipyard",
            "Acme Shipyard",
            TenantLifecycleStatus.Active,
            TenantDeploymentProfile.CreateNative(
                TenantDataTier.SharedSqlServer,
                "eu-west",
                new NativeRuntimeProfile("vault:tenants/acme/native-db", "https://api.example/v1")),
            new TenantPackEntitlements([], customizationPacks),
            new TenantMigrationState(TenantMigrationPhase.None, null, null, null),
            new TenantBillingStub("standard", 50));
}
