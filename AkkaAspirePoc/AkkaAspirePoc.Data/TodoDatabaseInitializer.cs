using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AkkaAspirePoc.Data;

public static class TodoDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TodoDbContext>>();

        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Todo database migrations applied.");
    }
}
