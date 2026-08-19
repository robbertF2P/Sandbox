using AkkaAspirePoc.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AkkaAspirePoc.Data.Migrations;

public sealed class TodoDbContextFactory : IDesignTimeDbContextFactory<TodoDbContext>
{
    public TodoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TodoDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=AkkaAspirePoc;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True",
            sql => sql.MigrationsAssembly(typeof(TodoDbContextFactory).Assembly.GetName().Name));

        return new TodoDbContext(optionsBuilder.Options);
    }
}
