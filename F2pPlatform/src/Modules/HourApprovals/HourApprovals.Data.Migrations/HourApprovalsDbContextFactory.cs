using HourApprovals.Infrastructure;
using HourApprovals.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HourApprovals.Data.Migrations;

public sealed class HourApprovalsDbContextFactory : IDesignTimeDbContextFactory<HourApprovalsDbContext>
{
    public HourApprovalsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HourApprovalsDbContext>();
        optionsBuilder.UseSqlServer(
            DependencyInjection.DefaultConnectionString,
            sql => sql.MigrationsAssembly(typeof(HourApprovalsDbContextFactory).Assembly.GetName().Name));

        return new HourApprovalsDbContext(optionsBuilder.Options);
    }
}
