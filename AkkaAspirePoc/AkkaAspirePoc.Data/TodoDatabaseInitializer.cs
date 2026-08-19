using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AkkaAspirePoc.Data;

public static class TodoDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TodoDbContext>>();

        if (string.Equals(config["Demo:UseSqlite"], "true", StringComparison.OrdinalIgnoreCase))
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
            logger.LogInformation("Todo demo database created (SQLite).");
            return;
        }

        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Todo database migrations applied.");
    }
}
