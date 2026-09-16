using Akka.Actor;
using Akka.Hosting;
using AwesomeAssertions;
using Contracts.Infrastructure.Connectors;
using Domain.Model;
using Domain.Model.Sync;
using Floor2Plan.Connectors.P6;
using Floor2Plan.Connectors.P6.Actors;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Configuration;
using Floor2Plan.TestUtility.Common.Framework;
using Infrastructure.Akka.Contracts;
using Infrastructure.Process.Contracts.Scope;
using Microsoft.Extensions.Options;
using Contracts.Model.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Floor2Plan.UnitTest.Connectors.P6
{
    public class P6ConnectorTest : global::Akka.Hosting.TestKit.TestKit
    {
        public P6ConnectorTest(ITestOutputHelper output)
            : base(nameof(P6ConnectorTest), output)
        {
        }

        protected override void ConfigureAkka(AkkaConfigurationBuilder builder, IServiceProvider provider)
        {
            _ = builder;
            _ = provider;
        }

        [F2PFact]
        public async Task GetConnectorConfigurationAsync_EnablesSyncSaveAndRawData()
        {
            var api = CreateApi();
            var target = CreateTarget(api, CreateSelectionStore().Object);

            var configuration = await target.GetConnectorConfigurationAsync();

            configuration.CanDownloadRawData.Should().BeTrue();
            configuration.CanSync.Should().BeTrue();
            configuration.CanSave.Should().BeTrue();
        }

        [F2PFact]
        public async Task GetConnectorConfigurationAsync_ReturnsProjectsWithPersistedSelection()
        {
            var api = CreateApi();
            var target = CreateTarget(api, CreateSelectionStore("10001").Object);

            var configuration = await target.GetConnectorConfigurationAsync();

            var group = configuration.PropertyGroups.Single();
            group.Key.Should().Be(P6Connector.ActiveProjectsGroupKey);
            group.InputGroupType.Should().Be(InputGroupType.GridCheckbox);

            var property = group.Properties.Cast<ConfigurationProperty>().Single();
            property.Name.Should().Be("10001");
            property.Description.Should().Be("DEMO-001 - Demo Construction Project");
            property.InputType.Should().Be(InputType.Checkbox);
            property.Values.Single().Should().Be("True");
        }

        [F2PFact]
        public async Task GetConnectorConfigurationAsync_LeavesUnselectedProjectsUnchecked()
        {
            var api = CreateApi();
            var target = CreateTarget(api, CreateSelectionStore().Object);

            var configuration = await target.GetConnectorConfigurationAsync();

            var property = configuration.PropertyGroups.Single().Properties.Cast<ConfigurationProperty>().Single();
            property.Values.Single().Should().Be("False");
        }

        [F2PFact]
        public async Task SaveConnectorConfigurationAsync_PersistsOnlyCheckedProjects()
        {
            var api = CreateApi();
            var selectionStore = CreateSelectionStore();
            var target = CreateTarget(api, selectionStore.Object);

            await target.SaveConnectorConfigurationAsync(new ConnectorConfiguration
            {
                PropertyGroups =
                [
                    new ConfigurationPropertyGroup
                    {
                        Key = P6Connector.ActiveProjectsGroupKey,
                        Properties =
                        [
                            new ConfigurationProperty { Name = "DEMO-001", InputType = InputType.Checkbox, Values = ["true"] },
                            new ConfigurationProperty { Name = "DEMO-002", InputType = InputType.Checkbox, Values = ["false"] }
                        ]
                    }
                ]
            });

            selectionStore.Verify(
                x => x.SaveSelectedProjectIdsAsync(It.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { "DEMO-001" }))),
                Times.Once);
        }

        [F2PFact]
        public async Task SaveConnectorConfigurationAsync_PersistsEmptySelectionWhenNothingIsChecked()
        {
            var api = CreateApi();
            var selectionStore = CreateSelectionStore();
            var target = CreateTarget(api, selectionStore.Object);

            await target.SaveConnectorConfigurationAsync(new ConnectorConfiguration
            {
                PropertyGroups =
                [
                    new ConfigurationPropertyGroup
                    {
                        Key = P6Connector.ActiveProjectsGroupKey,
                        Properties =
                        [
                            new ConfigurationProperty { Name = "DEMO-001", InputType = InputType.Checkbox, Values = ["false"] }
                        ]
                    }
                ]
            });

            selectionStore.Verify(
                x => x.SaveSelectedProjectIdsAsync(It.Is<IEnumerable<string>>(ids => !ids.Any())),
                Times.Once);
        }

        [F2PFact]
        public async Task GetRawDataAsync_ReturnsActorResult()
        {
            var api = CreateApi();
            var target = CreateTarget(api, CreateSelectionStore().Object);

            var (memoryStream, fileName, mimeType) = await target.GetRawDataAsync(new SyncParams());

            Encoding.UTF8.GetString(memoryStream.ToArray()).Should().Contain("Demo Construction Project");
            Encoding.UTF8.GetString(memoryStream.ToArray()).Should().Contain("SummaryActivityCount");
            fileName.Should().Be("p6-raw-data.json");
            mimeType.Should().Be("application/json");
        }

        [F2PFact]
        public async Task SyncAllAsync_LogsSyncMetrics()
        {
            var api = CreateApi();
            var processLogger = new Mock<IProcessLogger<P6Connector>>();
            var target = CreateTarget(api, CreateSelectionStore("1").Object, processLogger.Object);

            await target.SyncAllAsync([]);

            processLogger.Verify(
                x => x.LogRange(It.Is<IEnumerable<SyncLogMessageDto>>(
                    messages => messages.Any(m => m.SyncType == SyncType.Project && m.Quantity == 1))),
                Times.Once);
            processLogger.Verify(
                x => x.Log(It.Is<SyncLogMessageDto>(m => m.SyncInformation == SyncInformation.Success)),
                Times.Once);
        }

        private P6Connector CreateTarget(
            Mock<IP6RestApi> api,
            IP6ProjectSelectionStore projectSelectionStore,
            IProcessLogger<P6Connector> processLogger = null)
        {
            var p6Actor = Sys.ActorOf(P6Actor.Props(api.Object, CreateAuthOptions()));
            var facade = new Mock<IActorSystemFacade>();
            facade
                .Setup(x => x.RegisterActor(P6Actor.ActorName, It.IsAny<Props>()))
                .ReturnsAsync(p6Actor);

            return new P6Connector(
                facade.Object,
                api.Object,
                Options.Create(CreateAuthOptions()),
                processLogger ?? Mock.Of<IProcessLogger<P6Connector>>(),
                projectSelectionStore);
        }

        private static Mock<IP6ProjectSelectionStore> CreateSelectionStore(params string[] selectedProjectIds)
        {
            var store = new Mock<IP6ProjectSelectionStore>();
            store.Setup(x => x.GetSelectedProjectIdsAsync()).ReturnsAsync(selectedProjectIds);
            return store;
        }

        private static Mock<IP6RestApi> CreateApi()
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

        private static P6AuthOptions CreateAuthOptions()
        {
            return new P6AuthOptions
            {
                Username = "user",
                Password = "pass",
                DatabaseName = "PMDB",
                SessionIdleTimeout = TimeSpan.FromMilliseconds(100)
            };
        }
    }
}
