using MetaTable.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MetaTable.Tests.Support;

public sealed class MetaTableTestContextFactory(string connectionString)
{
    public MetaTableDbContext CreateDbContext()
    {
        DbContextOptionsBuilder<MetaTableDbContext> builder = new();
        builder.UseSqlite(connectionString);
        MetaTableDbContext context = new(builder.Options);
        context.Database.EnsureCreated();
        return context;
    }
}
