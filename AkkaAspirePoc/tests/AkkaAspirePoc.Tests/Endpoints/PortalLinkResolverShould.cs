using AkkaAspirePoc.Api.Endpoints;
using Microsoft.Extensions.Configuration;

namespace AkkaAspirePoc.Tests.Endpoints;

public sealed class PortalLinkResolverShould
{
    [Test]
    public async Task ResolveAspireDashboardUrl_uses_configured_url()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Aspire:DashboardUrl"] = "https://localhost:17261"
            })
            .Build();

        var url = PortalLinkResolver.ResolveAspireDashboardUrl(config);

        await Assert.That(url).IsEqualTo("https://localhost:17261");
    }

    [Test]
    public async Task ResolveAspireDashboardUrl_ignores_unresolved_parameter_placeholders()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Aspire:DashboardUrl"] = "{aspire-dashboard-url}"
            })
            .Build();

        var url = PortalLinkResolver.ResolveAspireDashboardUrl(config);

        await Assert.That(url).IsNull();
    }

    [Test]
    public async Task ResolveAspireDashboardUrl_falls_back_when_running_under_aspire()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "https://localhost:21252"
            })
            .Build();

        var url = PortalLinkResolver.ResolveAspireDashboardUrl(config);

        await Assert.That(url).IsEqualTo("https://localhost:17261");
    }

    [Test]
    public async Task SentryUnavailableStatus_explains_optional_configuration()
    {
        var config = new ConfigurationBuilder().Build();

        var status = PortalLinkResolver.SentryUnavailableStatus(config);

        await Assert.That(status).Contains("Optional");
    }
}
