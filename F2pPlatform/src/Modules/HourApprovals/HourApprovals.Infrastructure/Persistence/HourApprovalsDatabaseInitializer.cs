using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HourApprovals.Infrastructure.Persistence;

public sealed class HourApprovalsDatabaseInitializer(
    HourApprovalsDbContext dbContext,
    ILogger<HourApprovalsDatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var isSqlite = dbContext.Database.IsSqlite();

        if (isSqlite)
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        await HourApprovalsDataSeeder.SeedAsync(dbContext, cancellationToken);
        logger.LogInformation("Hour approvals database initialized");
    }
}
