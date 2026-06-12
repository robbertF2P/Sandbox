using Akka.Actor;
using Akka.TestKit;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Actors.Data;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Api.Tests.Actors;

public sealed class DataManagerActorTests : ActorTestBase<DataManagerActorTests>, IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private IDbContextFactory<ImportDbContext> _dbContextFactory = null!;

    public DataManagerActorTests(ITestOutputHelper output)
        : base(output)
    {
    }

    public async ValueTask InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ImportDbContext>()
            .UseSqlite(_connection)
            .Options;

        await using (var db = new ImportDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            await db.Database.ExecuteSqlRawAsync(
                "ALTER TABLE Components ADD COLUMN IsTemplate INTEGER NOT NULL DEFAULT 0");
        }

        _dbContextFactory = new TestDbContextFactory(options);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await ShutdownAsync();
    }

    [Fact]
    public async Task PersistImportWithModelCommand_PublishesImportPersistedAndReplies()
    {
        var probe = CreateTestProbe();
        Sys.EventStream.Subscribe(probe.Ref, typeof(ImportPersisted));

        var dataManager = Sys.ActorOf(DataManagerActor.Props(_dbContextFactory), "data-manager");
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
