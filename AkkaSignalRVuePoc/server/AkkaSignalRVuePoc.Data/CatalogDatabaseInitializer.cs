using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AkkaSignalRVuePoc.Data;

public static class CatalogDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(CatalogDatabaseInitializer));
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<CatalogDbContext>>();

        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        if (IsSqlite(db))
        {
            logger.LogInformation("Ensuring SQLite catalog database is created");
            await db.Database.EnsureCreatedAsync(cancellationToken);
            return;
        }

        logger.LogInformation("Applying catalog database migrations");
        await db.Database.MigrateAsync(cancellationToken);
    }

    private static bool IsSqlite(CatalogDbContext db) =>
        db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
}
