using Akka.Actor;
using AkkaSignalRVuePoc.Api.Services;
using AkkaSignalRVuePoc.Api.Tests.Data;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Core.Actors;
using AkkaSignalRVuePoc.Core.Publishing;
using Moq;

namespace AkkaSignalRVuePoc.Api.Tests.Actors;

public sealed class BackgroundProcessFlowTests : ActorTestBase<BackgroundProcessFlowTests>, IClassFixture<CatalogDatabaseFixture>
{
    private static readonly BackgroundProcessTiming TestTiming = new(
      Duration: TimeSpan.FromMilliseconds(300),
      BusySignalInterval: TimeSpan.FromMilliseconds(80));

    private readonly CatalogDatabaseFixture _database;

    public BackgroundProcessFlowTests(ITestOutputHelper output, CatalogDatabaseFixture database)
        : base(output)
    {
        _database = database;
    }

    [Fact]
    public async Task Facade_start_command_reaches_publish_service_with_busy_and_finished_signals()
    {
        var publisherMock = new Mock<ISignalrHubWrapper>();
        var publishedMessages = new List<PushMessage>();
        publisherMock
            .Setup(publisher => publisher.PublishActorMessageAsync(It.IsAny<PushMessage>()))
            .Callback<PushMessage>(publishedMessages.Add)
            .Returns(Task.CompletedTask);

        var hubPushActor = Sys.ActorOf(
            SignalRHubActor.Props(publisherMock.Object),
            "signalr-hub-push");
        var rootActor = Sys.ActorOf(
            RootActor.Props(hubPushActor, _database.Factory, TestTiming),
            "live-message-root");

        var commandFacade = new ActorSystemCommandFacade(rootActor);
        commandFacade.StartBackgroundProcess();

        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            var hasBusy = publishedMessages.Any(message =>
                message.Text.Contains("busy", StringComparison.OrdinalIgnoreCase));
            var hasFinished = publishedMessages.Any(message =>
                message.Text.Contains("finished", StringComparison.OrdinalIgnoreCase));

            if (hasBusy && hasFinished)
            {
                break;
            }

            await Task.Delay(50, TestContext.Current.CancellationToken);
        }

        publisherMock.Verify(
            publisher => publisher.PublishActorMessageAsync(It.Is<PushMessage>(message =>
                message.Text.Contains("busy", StringComparison.OrdinalIgnoreCase))),
            Times.AtLeastOnce);
        publisherMock.Verify(
            publisher => publisher.PublishActorMessageAsync(It.Is<PushMessage>(message =>
                message.Text.Contains("finished", StringComparison.OrdinalIgnoreCase))),
            Times.Once);

        await rootActor.GracefulStop(TimeSpan.FromSeconds(3));
    }
}
