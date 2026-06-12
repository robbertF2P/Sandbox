using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiImportActorPoc.Data;

public static class ImportDataServiceCollectionExtensions
{
    public const string ImportConnectionStringName = "Import";
    public const string MigrationsAssemblyName = "ApiImportActorPoc.Data.Migrations";

    public static IServiceCollection AddImportData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration.GetValue<ImportDatabaseProvider?>("Database:Provider")
            ?? ImportDatabaseProvider.Sqlite;
        var connectionString = configuration.GetConnectionString(ImportConnectionStringName)
            ?? "Data Source=import.db";

        services.AddDbContextFactory<ImportDbContext>(options =>
        {
            if (provider == ImportDatabaseProvider.Sqlite)
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
