using Floor2Plan.Connectors.P6;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Traditional;
using Infrastructure.Process.Contracts.Scope;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Floor2Plan.UnitTest.Connectors.P6.Traditional
{
    internal static class P6TraditionalTestSupport
    {
        internal static (IServiceScopeFactory ScopeFactory, Mock<IProcessLogger<P6TraditionalConnector>> ProcessLogger) CreateScopeFactory(
            Mock<IProcessLogger<P6TraditionalConnector>> processLogger = null)
        {
            var logger = processLogger ?? new Mock<IProcessLogger<P6TraditionalConnector>>();
            var services = new ServiceCollection();
            services.AddSingleton(logger.Object);
            var provider = services.BuildServiceProvider();
            return (provider.GetRequiredService<IServiceScopeFactory>(), logger);
        }

        internal static P6AuthOptions CreateAuthOptions()
        {
            return new P6AuthOptions
            {
                Username = "user",
                Password = "pass",
                DatabaseName = "PMDB",
                SessionIdleTimeout = TimeSpan.FromMilliseconds(100)
            };
        }

        internal static Mock<IP6RestApi> CreateApi()
        {
            var api = new Mock<IP6RestApi>();
            api.Setup(x => x.LoginAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<HttpContent>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Headers =
                    {
                        { "Set-Cookie", "JSESSIONID=abc123; Path=/p6ws; HttpOnly" }
                    }
                });
            api.Setup(x => x.GetProjectsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6ProjectRecord>
                {
                    new()
                    {
                        ObjectId = 10001,
                        Id = "DEMO-001",
                        Name = "Demo Construction Project",
                        Status = "Active",
                        SummaryActivityCount = 42
                    }
                });
            api.Setup(x => x.LogoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<HttpContent>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            return api;
        }

        internal static ILogger<T> NullLogger<T>() => NullLoggerFactory.Instance.CreateLogger<T>();
    }
}
