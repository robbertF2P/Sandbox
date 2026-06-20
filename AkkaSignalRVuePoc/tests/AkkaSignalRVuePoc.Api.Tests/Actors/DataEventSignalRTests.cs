using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Contracts.Notifications;
using AkkaSignalRVuePoc.Core.Actors.Data;
using AkkaSignalRVuePoc.Data;

namespace AkkaSignalRVuePoc.Api.Tests.Actors;

public sealed class DataEventSignalRTests : ActorDatabaseTestBase<DataEventSignalRTests>
{
    public DataEventSignalRTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public async Task Creating_project_publishes_ProjectCreated_data_event_to_SignalR()
    {
        var hubPush = CreateHubPushActor();
        var dataManager = Sys.ActorOf(DataManagerActor.Props(DatabaseFactory), "data-manager");

        var result = await ActorTestCorrelation.AskAsync<CreateProjectResult>(
            dataManager,
            new CreateProjectCommand(
                CatalogSeedData.AcmeOrganisationId,
                "Event Test Project",
                "Created for SignalR data event test"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.OrganisationExists);
        Assert.NotNull(result.Project);

        var call = await HubContext.ClientProxy.WaitForCallAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("dataEvent", call.Method);

        var notification = Assert.IsType<DataEventNotification>(Assert.Single(call.Arguments));
        Assert.Equal(nameof(ProjectCreated), notification.EventType);
        Assert.Equal(result.Project!.Id, notification.Project.Id);
        Assert.Equal("Event Test Project", notification.Project.Name);
    }

    [Fact]
    public async Task Updating_project_publishes_ProjectUpdated_data_event_to_SignalR()
    {
        var hubPush = CreateHubPushActor();
        var dataManager = Sys.ActorOf(DataManagerActor.Props(DatabaseFactory), "data-manager");

        var updated = await ActorTestCorrelation.AskAsync<UpdateProjectResult>(
            dataManager,
            new UpdateProjectCommand(
                CatalogSeedData.CustomerPortalProjectId,
                "Updated Portal",
                null), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(updated.Exists);
        Assert.NotNull(updated.Project);

        var call = await HubContext.ClientProxy.WaitForCallAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("dataEvent", call.Method);

        var notification = Assert.IsType<DataEventNotification>(Assert.Single(call.Arguments));
        Assert.Equal(nameof(ProjectUpdated), notification.EventType);
        Assert.Equal("Updated Portal", notification.Project.Name);
    }

    [Fact]
    public async Task Deleting_project_publishes_ProjectDeleted_data_event_to_SignalR()
    {
        var hubPush = CreateHubPushActor();
        var dataManager = Sys.ActorOf(DataManagerActor.Props(DatabaseFactory), "data-manager-delete");

        var created = await ActorTestCorrelation.AskAsync<CreateProjectResult>(
            dataManager,
            new CreateProjectCommand(
                CatalogSeedData.AcmeOrganisationId,
                "Delete Me",
                null), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(created.OrganisationExists);
        Assert.NotNull(created.Project);

        _ = await HubContext.ClientProxy.WaitForCallAsync(TimeSpan.FromSeconds(5));

        var deleted = await ActorTestCorrelation.AskAsync<DeleteProjectResult>(
            dataManager,
            new DeleteProjectCommand(created.Project!.Id),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(deleted.Exists);

        var call = await HubContext.ClientProxy.WaitForCallAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("dataEvent", call.Method);

        var notification = Assert.IsType<DataEventNotification>(Assert.Single(call.Arguments));
        Assert.Equal(nameof(ProjectDeleted), notification.EventType);
        Assert.Equal(created.Project.Id, notification.Project.Id);
    }
}
