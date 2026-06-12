using Akka.Actor;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Actors.Data;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ImportDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var db = new ImportDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
        }

        var dbContextFactory = new TestDbContextFactory(options);
        var probe = CreateTestProbe();
        Sys.EventStream.Subscribe(probe.Ref, typeof(ImportPersisted));

        var dataManager = Sys.ActorOf(DataManagerActor.Props(dbContextFactory), "data-manager");
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

    private sealed class TestDbContextFactory(DbContextOptions<ImportDbContext> options) : IDbContextFactory<ImportDbContext>
    {
        public ImportDbContext CreateDbContext() => new(options);

        public Task<ImportDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
