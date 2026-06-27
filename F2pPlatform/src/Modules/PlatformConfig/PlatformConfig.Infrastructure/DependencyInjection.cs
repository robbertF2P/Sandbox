using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlatformConfig.Application.Ports;
using PlatformConfig.Infrastructure.Persistence;
using PlatformConfig.Infrastructure.Runtime;

namespace PlatformConfig.Infrastructure;

public static class DependencyInjection
{
    public const string PlatformConfigConnectionStringName = "PlatformConfig";
    public const string MigrationsAssemblyName = "PlatformConfig.Data.Migrations";

    public const string DefaultConnectionString =
        "Server=localhost,1402;Database=F2pPlatform;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;Encrypt=False";

    public static IServiceCollection AddPlatformConfigInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<ITenantRuntimeContext, TenantRuntimeContext>();

        services.AddDbContext<PlatformConfigDbContext>((serviceProvider, options) =>
        {
            IHostEnvironment environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            var isTesting = environment.IsEnvironment("Testing");
            var useSqlite = isTesting || configuration.GetValue<bool>("PlatformConfig:UseSqlite");

            if (useSqlite)
            {
                var sqliteConnectionString = configuration["PlatformConfig:SqliteConnectionString"];

                if (string.IsNullOrWhiteSpace(sqliteConnectionString))
                {
                    var sqlitePath = configuration["PlatformConfig:SqlitePath"];
                    sqliteConnectionString = sqlitePath is not null
                        ? $"Data Source={sqlitePath}"
                        : isTesting
                            ? "Data Source=platform-config-tests;Mode=Memory;Cache=Shared"
                            : "Data Source=platform-config.db";
                }

                options.UseSqlite(sqliteConnectionString);
                return;
            }

            var connectionString = configuration.GetConnectionString(PlatformConfigConnectionStringName)
                ?? configuration.GetConnectionString("F2pPlatform")
                ?? DefaultConnectionString;

            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
        });

        services.AddScoped<ITenantConfigurationStore, EfTenantConfigurationStore>();
        services.AddScoped<PlatformConfigDatabaseInitializer>();
        services.AddHostedService<PlatformConfigDatabaseInitializerHostedService>();

        return services;
    }
}
