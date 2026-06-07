using EfCoreHoursNormalization.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCoreHoursNormalization;

public sealed class HoursDbContext(DbContextOptions<HoursDbContext> options) : DbContext(options)
{
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HoursDbContext).Assembly);
    }
}
