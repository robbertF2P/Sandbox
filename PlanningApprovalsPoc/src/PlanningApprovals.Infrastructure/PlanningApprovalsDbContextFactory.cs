using Microsoft.EntityFrameworkCore;

namespace PlanningApprovals.Infrastructure;

public sealed class PlanningApprovalsDbContextFactory(string connectionString)
{
    public PlanningApprovalsDbContext CreateDbContext()
    {
        DbContextOptions<PlanningApprovalsDbContext> options = new DbContextOptionsBuilder<PlanningApprovalsDbContext>()
            .UseSqlite(connectionString)
            .Options;

        PlanningApprovalsDbContext context = new(options);
        context.Database.EnsureCreated();
        return context;
    }
}
