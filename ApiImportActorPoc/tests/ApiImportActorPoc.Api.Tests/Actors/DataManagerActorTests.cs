using Akka.Actor;
using ApiImportActorPoc.Api.Tests.Infrastructure;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Actors.Data;
using ApiImportActorPoc.Core.Import;

namespace ApiImportActorPoc.Api.Tests.Actors;

public sealed class DataManagerActorTests : ActorTestBase<DataManagerActorTests>
{
    public DataManagerActorTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public async Task PersistImportWithModelCommand_PublishesImportPersistedAndReplies()
    {
        await using var database = new SqlServerTestDatabase();
        await database.InitializeAsync();

        var probe = CreateTestProbe();
        Sys.EventStream.Subscribe(probe.Ref, typeof(ImportPersisted));

        var dataManager = Sys.ActorOf(DataManagerActor.Props(database.Factory), "data-manager");
        var sessionId = Guid.NewGuid();
        var model = ProjectModelBuilder.Build(new ProjectImportPayload(
            "MV Actor Persist",
            [new ComponentImportPayload(null, "Block", true, null, null, null)])).Model;

        var result = await dataManager.Ask<PersistImportResult>(
            new PersistImportWithModelCommand(sessionId, model));

        Assert.True(result.Success);
        Assert.NotNull(result.ProjectId);

        var persisted = probe.ExpectMsg<ImportPersisted>();
        Assert.Equal(sessionId, persisted.SessionId);
        Assert.Equal(result.ProjectId, persisted.ProjectId);
    }
}
