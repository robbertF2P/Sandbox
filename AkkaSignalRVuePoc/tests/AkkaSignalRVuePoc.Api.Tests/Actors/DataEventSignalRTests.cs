using Akka.Actor;
using AkkaSignalRVuePoc.Api.Tests.Data;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Contracts.Notifications;
using AkkaSignalRVuePoc.Core.Actors.Data;
using AkkaSignalRVuePoc.Data;

namespace AkkaSignalRVuePoc.Api.Tests.Actors;

public sealed class DataEventSignalRTests : ActorTestBase<DataEventSignalRTests>, IClassFixture<CatalogDatabaseFixture>
{
    private readonly CatalogDatabaseFixture _database = new();

    public DataEventSignalRTests(ITestOutputHelper output, CatalogDatabaseFixture database)
        : base(output)
    {
        _database = database;

    }

    [Fact]
    public async Task Creating_project_publishes_ProjectCreated_data_event_to_SignalR()
    {
        var hubPush = CreateHubPushActor();
        var dataManager = Sys.ActorOf(DataManagerActor.Props(_database.Factory), "data-manager");

        var result = await dataManager.Ask<CreateProjectResult>(new CreateProjectCommand(
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
        var dataManager = Sys.ActorOf(DataManagerActor.Props(_database.Factory), "data-manager");

        var updated = await dataManager.Ask<UpdateProjectResult>(
            new UpdateProjectCommand(
                CatalogSeedData.CustomerPortalProjectId,
                "Updated Portal",
                null));

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
        var dataManager = Sys.ActorOf(DataManagerActor.Props(_database.Factory), "data-manager-delete");

        var created = await dataManager.Ask<CreateProjectResult>(
            new CreateProjectCommand(
                CatalogSeedData.AcmeOrganisationId,
                "Delete Me",
                null));

        Assert.True(created.OrganisationExists);
        Assert.NotNull(created.Project);

        _ = await HubContext.ClientProxy.WaitForCallAsync(TimeSpan.FromSeconds(5));

        var deleted = await dataManager.Ask<DeleteProjectResult>(
            new DeleteProjectCommand(created.Project!.Id));

        Assert.True(deleted.Exists);

        var call = await HubContext.ClientProxy.WaitForCallAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("dataEvent", call.Method);

        var notification = Assert.IsType<DataEventNotification>(Assert.Single(call.Arguments));
        Assert.Equal(nameof(ProjectDeleted), notification.EventType);
        Assert.Equal(created.Project.Id, notification.Project.Id);
    }
}
