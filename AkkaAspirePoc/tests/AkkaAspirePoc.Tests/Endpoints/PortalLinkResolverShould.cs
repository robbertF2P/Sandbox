using AkkaAspirePoc.Api.Endpoints;
using Microsoft.AspNetCore.Http;
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
    public async Task ResolveAspireDashboardUrlForRequest_uses_proxied_path_for_public_hosts()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Aspire:DashboardUrl"] = "https://localhost:17261",
                ["Aspire:DashboardLoginToken"] = "abc123"
            })
            .Build();

        var request = new DefaultHttpContext().Request;
        request.Host = new HostString("example.loca.lt");
        request.Scheme = "https";

        var url = PortalLinkResolver.ResolveAspireDashboardUrlForRequest(config, request);

        await Assert.That(url).IsEqualTo("https://example.loca.lt/aspire/login?t=abc123");
    }

    [Test]
    public async Task ResolveAspireDashboardUrlForRequest_keeps_local_url_for_local_hosts()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Aspire:DashboardUrl"] = "https://localhost:17261"
            })
            .Build();

        var request = new DefaultHttpContext().Request;
        request.Host = new HostString("localhost", 4200);
        request.Scheme = "http";

        var url = PortalLinkResolver.ResolveAspireDashboardUrlForRequest(config, request);

        await Assert.That(url).IsEqualTo("https://localhost:17261");
    }

    [Test]
    public async Task SentryUnavailableStatus_explains_optional_configuration()
    {
        var config = new ConfigurationBuilder().Build();

        var status = PortalLinkResolver.SentryUnavailableStatus(config);

        await Assert.That(status).Contains("Optional");
    }

    [Test]
    public async Task SanitizeLinkForPublicRequest_removes_localhost_urls_for_public_hosts()
    {
        var request = new DefaultHttpContext().Request;
        request.Host = new HostString("example.loca.lt");
        request.Scheme = "https";

        var sanitized = PortalLinkResolver.SanitizeLinkForPublicRequest("http://localhost:4200", request);

        await Assert.That(sanitized).IsNull();
    }

    [Test]
    public async Task SanitizeLinkForPublicRequest_keeps_localhost_urls_for_local_hosts()
    {
        var request = new DefaultHttpContext().Request;
        request.Host = new HostString("localhost", 4200);
        request.Scheme = "http";

        var sanitized = PortalLinkResolver.SanitizeLinkForPublicRequest("http://localhost:4200", request);

        await Assert.That(sanitized).IsEqualTo("http://localhost:4200");
    }

    [Test]
    public async Task SanitizeLinkForPublicRequest_keeps_public_urls_for_public_hosts()
    {
        var request = new DefaultHttpContext().Request;
        request.Host = new HostString("example.loca.lt");
        request.Scheme = "https";

        var sanitized = PortalLinkResolver.SanitizeLinkForPublicRequest("https://org.sentry.io/projects/demo/", request);

        await Assert.That(sanitized).IsEqualTo("https://org.sentry.io/projects/demo/");
    }
}
