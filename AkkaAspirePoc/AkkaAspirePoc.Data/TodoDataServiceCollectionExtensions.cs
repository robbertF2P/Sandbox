using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AkkaAspirePoc.Data;

public static class TodoDataServiceCollectionExtensions
{
    public static IServiceCollection AddTodoData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TodoDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly("AkkaAspirePoc.Data.Migrations")));

        return services;
    }
}
