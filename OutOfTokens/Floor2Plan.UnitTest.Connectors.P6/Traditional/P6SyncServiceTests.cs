using AwesomeAssertions;
using Contracts.Model.Enums;
using Domain.Model.Sync;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Sync;
using Floor2Plan.Connectors.P6.Traditional;
using Floor2Plan.Connectors.P6.Traditional.Services;
using Infrastructure.Process.Contracts.Scope;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Floor2Plan.UnitTest.Connectors.P6.Traditional
{
    public sealed class P6SyncServiceTests
    {
        [Fact]
        public async Task RunSync_FetchesProjectsAndWritesProgress()
        {
            var api = P6TraditionalTestSupport.CreateApi();
            var (_, processLogger) = P6TraditionalTestSupport.CreateScopeFactory();
            var target = CreateTarget(api, processLogger);

            var result = await target.RunSyncAsync(["1"], cancellationToken: TestContext.Current.CancellationToken);

            result.Succeeded.Should().BeTrue();
            result.ProjectCount.Should().Be(1);
            processLogger.Verify(
                x => x.Log(It.Is<SyncLogMessageDto>(m => m.Message.Contains("P6 sync started"))),
                Times.AtLeastOnce);
            processLogger.Verify(
                x => x.Log(It.Is<SyncLogMessageDto>(m => m.SyncInformation == SyncInformation.Success)),
                Times.Once);
        }

        [Fact]
        public async Task RunSync_WithEntityKindSubset_OnlyFetchesSelectedCatalogs()
        {
            var api = P6TraditionalTestSupport.CreateApi();
            var target = CreateTarget(
                api,
                syncOptions: new P6SyncOptions
                {
                    MaxConcurrency = 2,
                    EntityKinds = [P6EntityKind.Activities, P6EntityKind.Relationships]
                });

            var plans = new[]
            {
                new P6ProjectSyncPlan("1", new HashSet<P6EntityKind> { P6EntityKind.Activities, P6EntityKind.Relationships })
            };

            var result = await target.RunSyncAsync(["1"], plans, TestContext.Current.CancellationToken);

            result.Succeeded.Should().BeTrue();
            api.Verify(x => x.GetActivitiesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            api.Verify(x => x.GetRelationshipsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            api.Verify(x => x.GetWbsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void TryQueueSync_RejectsSecondCallUntilFirstCompletes()
        {
            var api = P6TraditionalTestSupport.CreateApi();
            api.Setup(x => x.GetActivitiesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Delay(200);
                    return new List<P6ActivityRecord>();
                });

            var target = CreateTarget(api);
            target.TryQueueSync(["1"], null, out _).Should().BeTrue();
            target.TryQueueSync(["1"], null, out var reason).Should().BeFalse();
            reason.Should().Contain("already running");
        }

        private static P6SyncService CreateTarget(
            Mock<IP6RestApi> api,
            Mock<IProcessLogger<P6TraditionalConnector>> processLogger = null,
            P6SyncOptions syncOptions = null)
        {
            var (_, logger) = P6TraditionalTestSupport.CreateScopeFactory(processLogger);
            var authOptions = Options.Create(P6TraditionalTestSupport.CreateAuthOptions());
            var sessionService = new P6SessionService(api.Object, authOptions);
            var catalogReader = new P6PagedCatalogReader(api.Object);
            var reporter = new P6SyncProgressReporter(logger.Object);
            var store = new P6RawDataStore();

            return new P6SyncService(
                sessionService,
                catalogReader,
                reporter,
                store,
                Options.Create(syncOptions ?? new P6SyncOptions { MaxConcurrency = 2 }),
                P6TraditionalTestSupport.NullLogger<P6SyncService>());
        }
    }
}
