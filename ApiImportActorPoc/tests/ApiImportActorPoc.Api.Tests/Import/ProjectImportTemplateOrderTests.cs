using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Api.Tests.Import;

public sealed class ProjectImportTemplateOrderTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private IDbContextFactory<ImportDbContext> _dbContextFactory = null!;
    private ProjectImportUpsertService _upsertService = null!;

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
        _upsertService = new ProjectImportUpsertService(_dbContextFactory);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task UpsertAsync_PersistsTemplateComponentsBeforeRegularSiblings()
    {
        var payload = new ProjectImportPayload(
            "MV Order Test",
            [
                new ComponentImportPayload(null, "Regular first in payload", false, null, null, null),
                new ComponentImportPayload(null, "Template second in payload", true, null, null, null)
            ]);

        var model = ProjectModelBuilder.Build(payload).Model;
        await _upsertService.UpsertAsync(model);

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var components = await db.Components
            .AsNoTracking()
            .OrderBy(component => component.Id)
            .ToListAsync();

        Assert.Equal(2, components.Count);
        Assert.True(components[0].IsTemplate);
        Assert.Equal("Template second in payload", components[0].Name);
        Assert.False(components[1].IsTemplate);
        Assert.Equal("Regular first in payload", components[1].Name);
    }

    private sealed class TestDbContextFactory(DbContextOptions<ImportDbContext> options) : IDbContextFactory<ImportDbContext>
    {
        public ImportDbContext CreateDbContext() => new(options);

        public Task<ImportDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
