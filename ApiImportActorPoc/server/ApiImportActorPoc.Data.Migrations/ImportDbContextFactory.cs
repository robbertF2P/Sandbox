using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ApiImportActorPoc.Data.Migrations;

public sealed class ImportDbContextFactory : IDesignTimeDbContextFactory<ImportDbContext>
{
    public ImportDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ImportDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1401;Database=ApiImportPoc;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True",
            sql => sql.MigrationsAssembly(typeof(ImportDbContextFactory).Assembly.GetName().Name));

        return new ImportDbContext(optionsBuilder.Options);
    }
}
