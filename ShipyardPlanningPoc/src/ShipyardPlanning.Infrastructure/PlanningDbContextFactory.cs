using Microsoft.EntityFrameworkCore;

namespace ShipyardPlanning.Infrastructure;

public sealed class PlanningDbContextFactory
{
    private readonly DbContextOptions<PlanningDbContext> _options;

    public PlanningDbContextFactory(string connectionString = "Data Source=:memory:")
    {
        _options = new DbContextOptionsBuilder<PlanningDbContext>()
            .UseSqlite(connectionString)
            .Options;
    }

    public PlanningDbContext CreateDbContext()
    {
        PlanningDbContext context = new(_options);
        context.Database.EnsureCreated();
        return context;
    }
}
