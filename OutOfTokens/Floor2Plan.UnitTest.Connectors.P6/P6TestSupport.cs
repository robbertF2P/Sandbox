using Floor2Plan.Connectors.P6;
using Infrastructure.Process.Contracts.Scope;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Floor2Plan.UnitTest.Connectors.P6
{
    internal static class P6TestSupport
    {
        internal static (IServiceScopeFactory ScopeFactory, Mock<IProcessLogger<P6Connector>> ProcessLogger) CreateScopeFactory(
            Mock<IProcessLogger<P6Connector>> processLogger = null)
        {
            var logger = processLogger ?? new Mock<IProcessLogger<P6Connector>>();
            var services = new ServiceCollection();
            services.AddSingleton(logger.Object);
            var provider = services.BuildServiceProvider();
            return (provider.GetRequiredService<IServiceScopeFactory>(), logger);
        }
    }
}
