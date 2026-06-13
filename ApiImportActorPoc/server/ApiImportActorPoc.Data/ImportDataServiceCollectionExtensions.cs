using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiImportActorPoc.Data;

public static class ImportDataServiceCollectionExtensions
{
    public const string ImportConnectionStringName = "Import";
    public const string MigrationsAssemblyName = "ApiImportActorPoc.Data.Migrations";

    public const string DefaultConnectionString =
        "Server=localhost,1401;Database=ApiImportPoc;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True";

    public static IServiceCollection AddImportData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ImportConnectionStringName)
            ?? DefaultConnectionString;

        services.AddDbContextFactory<ImportDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
        });

        return services;
    }
}
