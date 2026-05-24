using AkkaSignalRVuePoc.Data;
using AkkaSignalRVuePoc.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AkkaSignalRVuePoc.Api.Tests.Data;

public sealed class SqliteCatalogDbContextFactory : IDbContextFactory<CatalogDbContext>, IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CatalogDbContext> _options;

    private SqliteCatalogDbContextFactory(SqliteConnection connection, DbContextOptions<CatalogDbContext> options)
    {
        _connection = connection;
        _options = options;
    }

    public static async Task<SqliteCatalogDbContextFactory> CreateAsync(CancellationToken cancellationToken = default)
    {
        var databaseName = $"catalog-actor-tests-{Guid.NewGuid():N}";
        var connectionString = $"Data Source={databaseName};Mode=Memory;Cache=Shared";
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new CatalogDbContext(options);
        await db.Database.EnsureCreatedAsync(cancellationToken);
        await SeedIfEmptyAsync(db, cancellationToken);

        return new SqliteCatalogDbContextFactory(connection, options);
    }

    private static async Task SeedIfEmptyAsync(CatalogDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Organisations.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Organisations.AddRange(
            new Organisation
            {
                Id = CatalogSeedData.AcmeOrganisationId,
                Name = "Acme Corp",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Organisation
            {
                Id = CatalogSeedData.DrivenItOrganisationId,
                Name = "Driven IT",
                CreatedAt = DateTimeOffset.UtcNow
            });

        db.Projects.AddRange(
            new Project
            {
                Id = CatalogSeedData.CustomerPortalProjectId,
                OrganisationId = CatalogSeedData.AcmeOrganisationId,
                Name = "Customer Portal",
                Description = "Public-facing web application",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Project
            {
                Id = CatalogSeedData.AkkaPocProjectId,
                OrganisationId = CatalogSeedData.DrivenItOrganisationId,
                Name = "Akka SignalR POC",
                Description = "Demonstration of Akka.NET, SignalR, and Vue",
                CreatedAt = DateTimeOffset.UtcNow
            });

        await db.SaveChangesAsync(cancellationToken);
    }

    public CatalogDbContext CreateDbContext() => new(_options);

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
