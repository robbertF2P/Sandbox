using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AkkaSignalRVuePoc.Data;

public static class CatalogDataServiceCollectionExtensions
{
    public const string CatalogConnectionStringName = "Catalog";
    public const string MigrationsAssemblyName = "AkkaSignalRVuePoc.Data.Migrations";

    public static IServiceCollection AddCatalogData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration.GetValue<CatalogDatabaseProvider?>("Database:Provider")
            ?? CatalogDatabaseProvider.SqlServer;
        var connectionString = configuration.GetConnectionString(CatalogConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{CatalogConnectionStringName}' is not configured.");

        services.AddDbContextFactory<CatalogDbContext>(options =>
        {
            if (provider == CatalogDatabaseProvider.Sqlite)
            {
                options.UseSqlite(connectionString, sqlite =>
                    sqlite.MigrationsAssembly(MigrationsAssemblyName));
            }
            else
            {
                options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(MigrationsAssemblyName));
            }
        });

        return services;
    }
}
