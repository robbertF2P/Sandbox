using Akka.Actor;
using AwesomeAssertions;
using Floor2Plan.TestUtility.Common.Akka;
using Floor2Plan.TestUtility.Common.Framework;
using Floor2Plan.Connectors.P6.Actors;
using Floor2Plan.Connectors.P6;
using Floor2Plan.Connectors.P6.Api;
using Contracts.Model.Enums;
using Domain.Model.Sync;
using Floor2Plan.Connectors.P6.Api.Models;
using Floor2Plan.Connectors.P6.Messages;
using Floor2Plan.Connectors.P6.Sync;
using Infrastructure.Process.Contracts.Scope;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Floor2Plan.UnitTest.Connectors.P6
{
    [Collection("OutOfTokens.P6")]
    public class P6ActorTest : AkkaSerilogTestKit
    {
        public P6ActorTest(ITestOutputHelper output)
            : base(nameof(P6ActorTest), output)
        {
        }

        [F2PFact]
        public async Task GetP6RawData_ReturnsApiClientResult()
        {
            var api = CreateApi();
            var actor = Sys.ActorOf(P6Actor.Props(api.Object, CreateAuthOptions()));

            var result = await actor.Ask<P6RawData>(
                new GetP6RawData(),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);

            Encoding.UTF8.GetString(result.Content.ToArray()).Should().Contain("Demo Construction Project");
            Encoding.UTF8.GetString(result.Content.ToArray()).Should().Contain("SummaryActivityCount");
            Encoding.UTF8.GetString(result.Content.ToArray()).Should().Contain("ComputedActivityCount");
            result.FileName.Should().Be("p6-raw-data.json");
            result.MimeType.Should().Be("application/json");
            api.Verify(x => x.GetProjectsAsync(
                    "JSESSIONID=abc123",
                    IP6RestApi.ProjectSummaryFields,
                    IP6RestApi.ProjectSummaryOrderBy,
                    IP6RestApi.ProjectSummaryFilter,
                    IP6RestApi.ProjectSummaryPageSize,
                    0,
                    It.IsAny<CancellationToken>()),
                Times.Once);
            api.Verify(x => x.LoginAsync("dXNlcjpwYXNz", "PMDB", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
            api.Verify(x => x.LogoutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Never);

            await Task.Delay(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);

            api.Verify(x => x.LogoutAsync("JSESSIONID=abc123", It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Once);
            api.Verify(x => x.GetWbsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            api.Verify(x => x.GetActivitiesAsync(
                    "JSESSIONID=abc123",
                    "ObjectId,ProjectObjectId",
                    "ProjectObjectId=10001",
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Once,
                "raw data fetches one lightweight activity count per project, not per entity kind");
            api.Verify(x => x.GetResourcesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            api.Verify(x => x.GetResourceAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            api.Verify(x => x.GetRelationshipsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [F2PFact]
        public async Task StartP6Sync_FansOutCatalogRequestsAndAggregatesCounts()
        {
            var api = CreateApi();
            api.Setup(x => x.GetWbsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6WbsRecord> { new() });
            api.Setup(x => x.GetActivitiesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6ActivityRecord> { new() { ObjectId = 1 }, new() { ObjectId = 2 } });
            api.Setup(x => x.GetResourcesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6ResourceRecord> { new() });
            api.Setup(x => x.GetResourceAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6ResourceAssignmentRecord> { new() });
            api.Setup(x => x.GetRelationshipsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6RelationshipRecord> { new() { ObjectId = 1 }, new() { ObjectId = 2 }, new() { ObjectId = 3 } });

            var actor = Sys.ActorOf(P6Actor.Props(api.Object, CreateAuthOptions(), new P6SyncOptions { MaxConcurrency = 2 }));

            var result = await actor.Ask<P6SyncResult>(
                new StartP6Sync(new[] { "1" }),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);

            result.Succeeded.Should().BeTrue();
            result.ProjectCount.Should().Be(1);
            result.WbsCount.Should().Be(1);
            result.ActivityCount.Should().Be(2);
            result.ResourceCount.Should().Be(1);
            result.ResourceAssignmentCount.Should().Be(1);
            result.RelationshipCount.Should().Be(3);
            result.TotalRecordCount.Should().Be(9);
            api.Verify(x => x.GetProjectsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Once,
                "the Projects catalog kind still fetches project details once per project, but there is no separate ObjectId-resolution call");
        }

        [F2PFact]
        public async Task StartP6Sync_FollowsPagingUntilPartialPage()
        {
            var api = CreateApi();
            api.Setup(x => x.GetActivitiesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.Is<int>(offset => offset == 0),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6ActivityRecord> { new() { ObjectId = 1 }, new() { ObjectId = 2 } });
            api.Setup(x => x.GetActivitiesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.Is<int>(offset => offset == 2),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6ActivityRecord> { new() { ObjectId = 3 }, new() { ObjectId = 4 } });
            api.Setup(x => x.GetActivitiesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.Is<int>(offset => offset == 4),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6ActivityRecord> { new() { ObjectId = 5 } });

            var actor = Sys.ActorOf(P6Actor.Props(
                api.Object,
                CreateAuthOptions(),
                new P6SyncOptions { MaxConcurrency = 2, PageSize = 2 }));

            var result = await actor.Ask<P6SyncResult>(
                new StartP6Sync(new[] { "1" }),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);

            result.Succeeded.Should().BeTrue();
            result.ActivityCount.Should().Be(5);
            api.Verify(x => x.GetActivitiesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    2,
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(3));
        }

        [F2PFact]
        public async Task StartP6Sync_ReportsFailingCatalogWithoutLosingOtherCounts()
        {
            var api = CreateApi();
            api.Setup(x => x.GetRelationshipsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("relationship endpoint exploded"));

            var actor = Sys.ActorOf(P6Actor.Props(api.Object, CreateAuthOptions(), new P6SyncOptions { MaxConcurrency = 3 }));

            var result = await actor.Ask<P6SyncResult>(
                new StartP6Sync(new[] { "1" }),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);

            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Should().Contain("relationship endpoint exploded");
            result.ProjectCount.Should().Be(1);
            result.RelationshipCount.Should().Be(0);
        }

        [F2PFact]
        public async Task StartP6Sync_StoresFetchedRecordsInRawDataStore()
        {
            var api = CreateApi();
            var actor = Sys.ActorOf(P6Actor.Props(api.Object, CreateAuthOptions(), new P6SyncOptions { MaxConcurrency = 3 }));
            var store = await ResolveStoreAsync(actor);

            await actor.Ask<P6SyncResult>(
                new StartP6Sync(new[] { "1" }),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);

            var counts = await store.Ask<P6RawDataStoreActor.RawDataCounts>(
                new P6RawDataStoreActor.GetRawDataCounts(),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
            counts.GetCount(P6EntityKind.Projects).Should().Be(1);

            var projects = await store.Ask<P6RawDataStoreActor.RawData>(
                new P6RawDataStoreActor.GetRawData(P6EntityKind.Projects),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
            projects.Records.Should().ContainSingle()
                .Which.Should().BeOfType<P6ProjectRecord>()
                .Which.Name.Should().Be("Demo Construction Project");
        }

        [F2PFact]
        public async Task StartP6Sync_PublishesProgressToScopedProcessLogger()
        {
            var api = CreateApi();
            var processLogger = new CapturingProcessLogger();
            var services = new ServiceCollection();
            services.AddSingleton<IProcessLogger<P6Connector>>(processLogger);
            var provider = services.BuildServiceProvider();
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

            var actor = Sys.ActorOf(P6Actor.Props(
                api.Object,
                CreateAuthOptions(),
                new P6SyncOptions { MaxConcurrency = 2 },
                scopeFactory));

            await actor.Ask<P6SyncResult>(
                new StartP6Sync(["1"]),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);

            await AwaitAssertAsync(
                () => processLogger.Messages.Should().Contain(m => m.Message.Contains("P6 sync started")),
                TimeSpan.FromSeconds(5),
                cancellationToken: TestContext.Current.CancellationToken);

            processLogger.Messages.Should().ContainSingle(m => m.SyncInformation == SyncInformation.Success);
        }

        private sealed class CapturingProcessLogger : IProcessLogger<P6Connector>
        {
            public List<SyncLogMessageDto> Messages { get; } = new();

            public void Log(SyncLogMessageDto message) => Messages.Add(message);

            public void LogRange(IEnumerable<SyncLogMessageDto> messages) => Messages.AddRange(messages);
        }

        [F2PFact]
        public async Task StartP6Sync_WithEntityKindSubset_OnlyFetchesSelectedCatalogs()
        {
            var api = CreateApi();
            var actor = Sys.ActorOf(P6Actor.Props(api.Object, CreateAuthOptions(), new P6SyncOptions { MaxConcurrency = 2 }));
            var plans = new[]
            {
                new P6ProjectSyncPlan(
                    "1",
                    new HashSet<P6EntityKind> { P6EntityKind.Activities, P6EntityKind.Relationships })
            };

            var result = await actor.Ask<P6SyncResult>(
                new StartP6Sync(["1"], plans),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);

            result.Succeeded.Should().BeTrue();
            result.ActivityCount.Should().Be(0);
            result.RelationshipCount.Should().Be(0);
            api.Verify(x => x.GetActivitiesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            api.Verify(x => x.GetRelationshipsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            api.Verify(x => x.GetWbsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            api.Verify(x => x.GetResourcesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            api.Verify(x => x.GetResourceAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            api.Verify(x => x.GetProjectsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [F2PFact]
        public async Task StartP6Sync_WithAdditionalFilter_PassesFilterToApi()
        {
            var api = CreateApi();
            var actor = Sys.ActorOf(P6Actor.Props(api.Object, CreateAuthOptions(), new P6SyncOptions { MaxConcurrency = 2 }));
            var plans = new[]
            {
                new P6ProjectSyncPlan(
                    "1",
                    new HashSet<P6EntityKind> { P6EntityKind.Activities },
                    "LastUpdateDate>2026-03-01T00:00:00")
            };

            await actor.Ask<P6SyncResult>(
                new StartP6Sync(["1"], plans),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);

            api.Verify(x => x.GetActivitiesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    "ProjectObjectId=1;LastUpdateDate>2026-03-01T00:00:00",
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [F2PFact]
        public async Task StartP6Sync_WithMixedSyncPlans_AppliesPerProjectScopes()
        {
            var api = CreateApi();
            api.Setup(x => x.GetWbsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    "ProjectObjectId=2",
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6WbsRecord> { new() });

            var actor = Sys.ActorOf(P6Actor.Props(api.Object, CreateAuthOptions(), new P6SyncOptions { MaxConcurrency = 2 }));
            var plans = new[]
            {
                P6ProjectSyncPlan.Full("1"),
                new P6ProjectSyncPlan("2", new HashSet<P6EntityKind> { P6EntityKind.Wbs })
            };

            var result = await actor.Ask<P6SyncResult>(
                new StartP6Sync(["1", "2"], plans),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);

            result.Succeeded.Should().BeTrue();
            result.WbsCount.Should().Be(1);
            api.Verify(x => x.GetWbsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    "ProjectObjectId=2",
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            api.Verify(x => x.GetProjectsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Once,
                "full plan for project 1 fetches Projects; WBS-only plan for project 2 does not");
        }

        [F2PFact]
        public async Task StartP6Sync_ClearsPreviousRunBeforeCollectingAgain()
        {
            var api = CreateApi();
            var actor = Sys.ActorOf(P6Actor.Props(api.Object, CreateAuthOptions(), new P6SyncOptions { MaxConcurrency = 3 }));
            var store = await ResolveStoreAsync(actor);

            store.Tell(new P6RawDataStoreActor.AppendRawData(
                P6EntityKind.Projects,
                new P6BaseRecord[] { new P6ProjectRecord { ObjectId = 999 } }));
            store.Tell(new P6RawDataStoreActor.AppendRawData(
                P6EntityKind.Wbs,
                new P6BaseRecord[] { new P6WbsRecord { ObjectId = 998 } }));

            await actor.Ask<P6SyncResult>(
                new StartP6Sync(new[] { "1" }),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);

            var counts = await store.Ask<P6RawDataStoreActor.RawDataCounts>(
                new P6RawDataStoreActor.GetRawDataCounts(),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
            counts.GetCount(P6EntityKind.Projects).Should().Be(1);
            counts.GetCount(P6EntityKind.Wbs).Should().Be(0);

            var projects = await store.Ask<P6RawDataStoreActor.RawData>(
                new P6RawDataStoreActor.GetRawData(P6EntityKind.Projects),
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
            projects.Records.Should().NotContain(x => x.ObjectId == 999);
        }

        private async Task<IActorRef> ResolveStoreAsync(IActorRef parent)
        {
            var selection = Sys.ActorSelection(parent.Path / P6RawDataStoreActor.ActorName);
            IActorRef store = null;

            // ActorOf returns before the parent's PreStart has created the child, so poll until it resolves.
            await AwaitAssertAsync(
                async () => store = await selection.ResolveOne(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken),
                TimeSpan.FromSeconds(5),
                cancellationToken: TestContext.Current.CancellationToken);

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
                        LastUpdateDate = "2026-09-10T14:22:00",
                        DataDate = "2026-09-10T00:00:00",
                        SummaryActivityCount = 42,
                        SummaryCompletedActivityCount = 12,
                        SummaryInProgressActivityCount = 20,
                        SummaryNotStartedActivityCount = 10
                    }
                });
            api.Setup(x => x.LogoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<HttpContent>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            api.Setup(x => x.GetWbsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6WbsRecord>());
            api.Setup(x => x.GetActivitiesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6ActivityRecord>());
            api.Setup(x => x.GetResourcesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6ResourceRecord>());
            api.Setup(x => x.GetResourceAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6ResourceAssignmentRecord>());
            api.Setup(x => x.GetRelationshipsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<P6RelationshipRecord>());

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
