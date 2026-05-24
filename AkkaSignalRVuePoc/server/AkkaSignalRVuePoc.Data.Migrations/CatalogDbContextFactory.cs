using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AkkaSignalRVuePoc.Data.Migrations;

public sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=AkkaSignalRPoc;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True",
            sql => sql.MigrationsAssembly(typeof(CatalogDbContextFactory).Assembly.GetName().Name));

        return new CatalogDbContext(optionsBuilder.Options);
    }
}
