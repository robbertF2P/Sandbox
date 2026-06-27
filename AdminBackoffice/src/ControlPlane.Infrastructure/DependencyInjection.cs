using ControlPlane.Application.Ports;
using ControlPlane.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.ControlPlane.Client;

namespace ControlPlane.Infrastructure;

public static class DependencyInjection
{
    public const string ControlPlaneConnectionStringName = "ControlPlane";
    public const string MigrationsAssemblyName = "ControlPlane.Data.Migrations";

    public const string DefaultConnectionString =
        "Server=localhost,1403;Database=ControlPlane;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;Encrypt=False";

    public static IServiceCollection AddControlPlaneInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ControlPlaneConnectionStringName)
            ?? DefaultConnectionString;

        services.AddDbContext<ControlPlaneDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName)));

        services.AddDbContextFactory<ControlPlaneDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName)));

        services.Configure<PlatformConfigurationOptions>(
            configuration.GetSection(PlatformConfigurationOptions.SectionName));

        services.AddPlatformConfigurationClient();
        services.AddScoped<ITenantRepository, EfTenantRepository>();

        return services;
    }
}
