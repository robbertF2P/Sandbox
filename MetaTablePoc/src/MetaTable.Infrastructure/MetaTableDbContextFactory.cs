using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MetaTable.Infrastructure;

public sealed class MetaTableDbContextFactory : IDesignTimeDbContextFactory<MetaTableDbContext>
{
    public MetaTableDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<MetaTableDbContext> builder = new();
        builder.UseSqlite("Data Source=metatable.db");
        return new MetaTableDbContext(builder.Options);
    }
}
