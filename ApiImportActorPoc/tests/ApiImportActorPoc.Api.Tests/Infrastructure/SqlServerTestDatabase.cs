using ApiImportActorPoc.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Api.Tests.Infrastructure;

public sealed class SqlServerTestDatabase : IAsyncDisposable
{
    public const string DefaultConnectionString =
        "Server=localhost,1401;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True";

    private readonly string _adminConnectionString;
    private readonly string _databaseName;
    private bool _initialized;

    public SqlServerTestDatabase(string? databaseName = null)
    {
        _databaseName = databaseName ?? $"ApiImportPoc_Test_{Guid.NewGuid():N}";
        var baseConnectionString = Environment.GetEnvironmentVariable("IMPORT_TEST_CONNECTION_STRING")
            ?? DefaultConnectionString;

        var builder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = _databaseName
        };
        ConnectionString = builder.ConnectionString;

        var adminBuilder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = "master"
        };
        _adminConnectionString = adminBuilder.ConnectionString;
    }

    public string ConnectionString { get; }

    public DbContextOptions<ImportDbContext> Options { get; private set; } = null!;

    public IDbContextFactory<ImportDbContext> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await CreateDatabaseAsync();

        Options = new DbContextOptionsBuilder<ImportDbContext>()
            .UseSqlServer(ConnectionString, sql =>
                sql.MigrationsAssembly("ApiImportActorPoc.Data.Migrations"))
            .Options;

        await using (var db = new ImportDbContext(Options))
        {
            await db.Database.MigrateAsync();
        }

        Factory = new TestDbContextFactory(Options);
        _initialized = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_initialized)
        {
            return;
        }

        try
        {
            await using var connection = new SqlConnection(_adminConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                IF DB_ID(N'{_databaseName}') IS NOT NULL
                BEGIN
                    ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{_databaseName}];
                END
                """;
            await command.ExecuteNonQueryAsync();
        }
        catch (SqlException)
        {
            // Best-effort cleanup for local/CI environments.
        }
    }

    private async Task CreateDatabaseAsync()
    {
        await using var connection = new SqlConnection(_adminConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            IF DB_ID(N'{_databaseName}') IS NULL
            BEGIN
                CREATE DATABASE [{_databaseName}];
            END
            """;
        await command.ExecuteNonQueryAsync();
    }

    private sealed class TestDbContextFactory(DbContextOptions<ImportDbContext> options) : IDbContextFactory<ImportDbContext>
    {
        public ImportDbContext CreateDbContext() => new(options);

        public Task<ImportDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
