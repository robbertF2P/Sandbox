using AkkaSignalRVuePoc.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AkkaSignalRVuePoc.Api.Tests.Integration;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _sqliteConnection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextFactory<CatalogDbContext>>();
            services.RemoveAll<DbContextOptions<CatalogDbContext>>();

            _sqliteConnection = new SqliteConnection("Data Source=catalog-api-tests;Mode=Memory;Cache=Shared");
            _sqliteConnection.Open();

            services.AddSingleton(_sqliteConnection);
            services.AddDbContextFactory<CatalogDbContext>(options =>
                options.UseSqlite(_sqliteConnection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sqliteConnection?.Dispose();
        }

        base.Dispose(disposing);
    }
}
