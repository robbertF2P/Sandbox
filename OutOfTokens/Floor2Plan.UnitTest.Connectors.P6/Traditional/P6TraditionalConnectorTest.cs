using AwesomeAssertions;
using Contracts.Infrastructure.Connectors;
using Contracts.Model.Enums;
using Domain.Model.Sync;
using Floor2Plan.Connectors.P6.Api;
using Domain.Model;
using Floor2Plan.Connectors.P6.Configuration;
using Floor2Plan.Connectors.P6.Traditional;
using Infrastructure.Process.Contracts.Scope;
using Floor2Plan.Connectors.P6.Traditional.Services;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Floor2Plan.UnitTest.Connectors.P6.Traditional
{
    public sealed class P6TraditionalConnectorTest
    {
        [Fact]
        public async Task GetConnectorConfigurationAsync_ReturnsProjectsWithPersistedSelection()
        {
            var api = P6TraditionalTestSupport.CreateApi();
            var target = CreateTarget(api, CreateSelectionStore("10001").Object);

            var configuration = await target.GetConnectorConfigurationAsync();

            var group = configuration.PropertyGroups.Single();
            group.Key.Should().Be(P6TraditionalConnector.ActiveProjectsGroupKey);
            var property = group.Properties.Cast<ConfigurationProperty>().Single();
            property.Name.Should().Be("10001");
            property.Values.Single().Should().Be("True");
        }

        [Fact]
        public async Task SyncAllAsync_ReturnsImmediatelyAndLogsProgressViaScopedLogger()
        {
            var api = P6TraditionalTestSupport.CreateApi();
            var (_, processLogger) = P6TraditionalTestSupport.CreateScopeFactory();
            var target = CreateTarget(api, CreateSelectionStore("1").Object, processLogger.Object);

            await target.SyncAllAsync([]);

            processLogger.Verify(
                x => x.Log(It.Is<SyncLogMessageDto>(m => m.Message.Contains("queued"))),
                Times.Once);

            await Task.Delay(500, TestContext.Current.CancellationToken);

            processLogger.Verify(
                x => x.Log(It.Is<SyncLogMessageDto>(m => m.SyncInformation == SyncInformation.Success)),
                Times.Once);
        }

        private static P6TraditionalConnector CreateTarget(
            Mock<IP6RestApi> api,
            IP6ProjectSelectionStore projectSelectionStore,
            IProcessLogger<P6TraditionalConnector> processLogger = null)
        {
            var mock = processLogger as Mock<IProcessLogger<P6TraditionalConnector>>
                ?? (processLogger != null ? Mock.Get(processLogger) : P6TraditionalTestSupport.CreateScopeFactory().ProcessLogger);

            var authOptions = Options.Create(P6TraditionalTestSupport.CreateAuthOptions());
            var sessionService = new P6SessionService(api.Object, authOptions);
            var catalogReader = new P6PagedCatalogReader(api.Object);
            var reporter = new P6SyncProgressReporter(mock.Object);
            var store = new P6RawDataStore();
            var syncService = new P6SyncService(
                sessionService,
                catalogReader,
                reporter,
                store,
                Options.Create(new P6SyncOptions { MaxConcurrency = 2 }),
                P6TraditionalTestSupport.NullLogger<P6SyncService>());
            var exporter = new P6RawDataExporter(sessionService, catalogReader, api.Object);

            return new P6TraditionalConnector(
                syncService,
                exporter,
                catalogReader,
                sessionService,
                mock.Object,
                projectSelectionStore);
        }

        private static Mock<IP6ProjectSelectionStore> CreateSelectionStore(params string[] selectedProjectIds)
        {
            var store = new Mock<IP6ProjectSelectionStore>();
            store.Setup(x => x.GetSelectedProjectIdsAsync()).ReturnsAsync(selectedProjectIds);
            return store;
        }
    }
}
