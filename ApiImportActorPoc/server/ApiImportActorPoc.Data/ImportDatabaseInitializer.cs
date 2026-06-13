using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApiImportActorPoc.Data;

public static class ImportDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ImportDatabaseInitializer");
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ImportDbContext>>();

        await using var db = await dbContextFactory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
        logger.LogInformation("SQL Server import database migrations applied.");
    }
}
