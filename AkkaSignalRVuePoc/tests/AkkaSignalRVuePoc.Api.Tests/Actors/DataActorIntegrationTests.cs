using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Core.Actors;
using AkkaSignalRVuePoc.Data;
using AkkaSignalRVuePoc.Api.Tests.Data;

namespace AkkaSignalRVuePoc.Api.Tests.Actors;

public sealed class DataActorIntegrationTests : ActorTestBase<DataActorIntegrationTests>, IClassFixture<CatalogDatabaseFixture>
{
    private readonly CatalogDatabaseFixture _database;

    public DataActorIntegrationTests(ITestOutputHelper output, CatalogDatabaseFixture database)
        : base(output)
    {
        _database = database;
    }

    [Fact]
    public async Task Data_manager_returns_seeded_organisations()
    {
        var dataManager = Sys.ActorOf(DataManagerActor.Props(_database.Factory), "data-manager");

        var result = await dataManager.Ask<GetAllOrganisationsResult>(
            new GetAllOrganisationsQuery(),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Organisations.Count);
        Assert.Contains(result.Organisations, organisation => organisation.Name == "Acme Corp");
        Assert.Contains(result.Organisations, organisation => organisation.Name == "Driven IT");
    }

    [Fact]
    public async Task Organisation_data_actor_creates_and_reads_organisation()
    {
        var actor = Sys.ActorOf(
            OrganisationDataActor.Props(_database.Factory),
            "organisation-data");

        var created = await actor.Ask<CreateOrganisationResult>(
            new CreateOrganisationCommand("New Org"),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("New Org", created.Organisation.Name);

        var fetched = await actor.Ask<GetOrganisationByIdResult>(
            new GetOrganisationByIdQuery(created.Organisation.Id),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(fetched.Organisation);
        Assert.Equal(created.Organisation.Id, fetched.Organisation.Id);
    }

    [Fact]
    public async Task Project_data_actor_creates_project_for_existing_organisation()
    {
        var actor = Sys.ActorOf(ProjectDataActor.Props(_database.Factory), "project-data");

        var created = await actor.Ask<CreateProjectResult>(
            new CreateProjectCommand(
                CatalogSeedData.AcmeOrganisationId,
                "Billing API",
                "Handles invoices"),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(created.OrganisationExists);
        Assert.NotNull(created.Project);
        Assert.Equal("Billing API", created.Project!.Name);

        var projects = await actor.Ask<GetProjectsByOrganisationResult>(
            new GetProjectsByOrganisationQuery(CatalogSeedData.AcmeOrganisationId),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(projects.OrganisationExists);
        Assert.Contains(projects.Projects, project => project.Name == "Billing API");
    }

    [Fact]
    public async Task Data_manager_updates_existing_project()
    {
        var dataManager = Sys.ActorOf(DataManagerActor.Props(_database.Factory), "data-manager-update");

        var updated = await dataManager.Ask<UpdateProjectResult>(
            new UpdateProjectCommand(
                CatalogSeedData.AkkaPocProjectId,
                "Renamed Akka POC",
                "Updated description"));

        Assert.True(updated.Exists);
        Assert.NotNull(updated.Project);
        Assert.Equal("Renamed Akka POC", updated.Project!.Name);
        Assert.Equal("Updated description", updated.Project.Description);
    }

    [Fact]
    public async Task Root_actor_routes_catalog_queries_to_data_manager()
    {
        var hubPushActor = CreateHubPushActor();
        var root = Sys.ActorOf(
            LiveMessageRootActor.Props(hubPushActor, _database.Factory),
            "live-message-root");

        var projects = await root.Ask<GetAllProjectsResult>(
            new GetAllProjectsQuery(),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Contains(projects.Projects, project => project.Name == "Akka SignalR POC");
    }
}
