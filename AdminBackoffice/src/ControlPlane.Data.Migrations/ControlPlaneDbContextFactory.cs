using ControlPlane.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ControlPlane.Data.Migrations;

public sealed class ControlPlaneDbContextFactory : IDesignTimeDbContextFactory<ControlPlaneDbContext>
{
    public ControlPlaneDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ControlPlaneDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1403;Database=ControlPlane;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;Encrypt=False",
            sql => sql.MigrationsAssembly(typeof(ControlPlaneDbContextFactory).Assembly.GetName().Name));

        return new ControlPlaneDbContext(optionsBuilder.Options);
    }
}
