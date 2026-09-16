using AwesomeAssertions;
using Contracts.Infrastructure.Connectors;
using Floor2Plan.Connectors.P6;
using Floor2Plan.Connectors.P6.Api;
using Microsoft.Extensions.Configuration;
using Floor2Plan.TestUtility.Common.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Refit;

namespace Floor2Plan.UnitTest.Connectors.P6
{
    public class AddP6ConnectorExtensionsTest
    {
        [F2PFact]
        public void AddP6Connector_RegistersConnectorServices()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();

            services.AddP6Connector(configuration);

            services.Should().ContainSingle(x =>
                x.ServiceType == typeof(IGenericConnector) &&
                x.ImplementationType == typeof(P6Connector) &&
                x.Lifetime == ServiceLifetime.Transient);
            services.Should().ContainSingle(x =>
                x.ServiceType == typeof(IP6RestApi));
            services.Should().ContainSingle(x => x.ServiceType == typeof(P6HttpLoggingHandler));
        }

        [F2PFact]
        public void AddP6Connector_ResolvesIP6RestApi_WithoutDependencyResolutionError()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();

            services.AddP6Connector(configuration);
            using var provider = services.BuildServiceProvider();

            var api = provider.GetRequiredService<IP6RestApi>();

            api.Should().NotBeNull();
        }

        [F2PFact]
        public async Task IP6RestApi_GetProjectsAsync_UsesP6RestApiProjectPath()
        {
            var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            });
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://p6.floor2plan.com/")
            };
            var target = RestService.For<IP6RestApi>(httpClient);

            _ = await target.GetProjectsAsync("JSESSIONID=abc123", cancellationToken: CancellationToken.None);

            handler.Request.RequestUri.ToString().Should().StartWith("https://p6.floor2plan.com/p6ws/restapi/project");
            handler.Request.RequestUri.Query.Should().NotContain("OrderBy");
            handler.Request.RequestUri.Query.Should().NotContain("Filter");
            handler.Request.Headers.GetValues("Cookie").Should().ContainSingle(x => x == "JSESSIONID=abc123");
        }

        [F2PFact]
        public async Task IP6RestApi_LoginAsync_UsesP6RestApiLoginPathAndAuthTokenHeader()
        {
            var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK));
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://p6.floor2plan.com/")
            };
            var target = RestService.For<IP6RestApi>(httpClient);

            _ = await target.LoginAsync("token", "PMDB", new ByteArrayContent(Array.Empty<byte>()), CancellationToken.None);

            handler.Request.RequestUri.ToString().Should().Be("https://p6.floor2plan.com/p6ws/restapi/login?DatabaseName=PMDB");
            handler.Request.Headers.GetValues("authToken").Should().ContainSingle(x => x == "token");
            handler.RequestContentLength.Should().Be(0);
        }

        [F2PFact]
        public async Task IP6RestApi_LogoutAsync_UsesP6RestApiLogoutPathAndCookieHeader()
        {
            var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK));
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://p6.floor2plan.com/")
            };
            var target = RestService.For<IP6RestApi>(httpClient);

            _ = await target.LogoutAsync("JSESSIONID=abc123", new ByteArrayContent(Array.Empty<byte>()), CancellationToken.None);

            handler.Request.RequestUri.ToString().Should().Be("https://p6.floor2plan.com/p6ws/restapi/logout");
            handler.Request.Headers.GetValues("Cookie").Should().ContainSingle(x => x == "JSESSIONID=abc123");
            handler.RequestContentLength.Should().Be(0);
        }

        private sealed class CapturingHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public CapturingHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            public HttpRequestMessage Request { get; private set; }

            public long? RequestContentLength { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _ = cancellationToken;
                Request = request;
                RequestContentLength = request.Content?.Headers.ContentLength;
                return Task.FromResult(_response);
            }
        }
    }
}
