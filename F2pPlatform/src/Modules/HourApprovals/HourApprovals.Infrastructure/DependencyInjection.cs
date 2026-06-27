using HourApprovals.Application.Persistence;
using HourApprovals.Application.Ports;
using HourApprovals.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HourApprovals.Infrastructure;

public static class DependencyInjection
{
    public const string HourApprovalsConnectionStringName = "HourApprovals";
    public const string MigrationsAssemblyName = "HourApprovals.Data.Migrations";

    public const string DefaultConnectionString =
        "Server=localhost,1402;Database=HourApprovals;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;Encrypt=False";

    public static IServiceCollection AddHourApprovalsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<HourApprovalsDbContext>((serviceProvider, options) =>
        {
            IHostEnvironment environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            var isTesting = environment.IsEnvironment("Testing");
            var useSqlite = isTesting || configuration.GetValue<bool>("HourApprovals:UseSqlite");

            if (useSqlite)
            {
                var sqliteConnectionString = configuration["HourApprovals:SqliteConnectionString"];

                if (string.IsNullOrWhiteSpace(sqliteConnectionString))
                {
                    var sqlitePath = configuration["HourApprovals:SqlitePath"];
                    sqliteConnectionString = sqlitePath is not null
                        ? $"Data Source={sqlitePath}"
                        : isTesting
                            ? "Data Source=hour-approvals-tests;Mode=Memory;Cache=Shared"
                            : "Data Source=hour-approvals.db";
                }

                options.UseSqlite(sqliteConnectionString);
                return;
            }

            var connectionString = configuration.GetConnectionString(HourApprovalsConnectionStringName)
                ?? configuration.GetConnectionString("F2pPlatform")
                ?? DefaultConnectionString;

            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
        });

        services.AddScoped<IHourApprovalsRepository, EfHourApprovalsRepository>();
        services.AddSingleton<IHourApprovalsFeatureGate, ConfigurationHourApprovalsFeatureGate>();
        services.AddScoped<HourApprovalsDatabaseInitializer>();
        services.AddHostedService<HourApprovalsDatabaseInitializerHostedService>();

        if (!services.Any(descriptor => descriptor.ServiceType == typeof(IHourApprovalsCustomizationPack)))
        {
            services.AddSingleton<IHourApprovalsCustomizationPack, DefaultHourApprovalsPack>();
        }

        return services;
    }
}
