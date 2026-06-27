using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PlatformConfig.Infrastructure;
using PlatformConfig.Infrastructure.Persistence;

namespace PlatformConfig.Data.Migrations;

public sealed class PlatformConfigDbContextFactory : IDesignTimeDbContextFactory<PlatformConfigDbContext>
{
    public PlatformConfigDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlatformConfigDbContext>();
        optionsBuilder.UseSqlServer(
            DependencyInjection.DefaultConnectionString,
            sql => sql.MigrationsAssembly(typeof(PlatformConfigDbContextFactory).Assembly.GetName().Name));

        return new PlatformConfigDbContext(optionsBuilder.Options);
    }
}
