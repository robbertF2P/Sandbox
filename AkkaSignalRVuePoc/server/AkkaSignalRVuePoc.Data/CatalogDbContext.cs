using AkkaSignalRVuePoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AkkaSignalRVuePoc.Data;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Organisation> Organisations => Set<Organisation>();

    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        CatalogSeedData.Apply(modelBuilder);
    }
}
