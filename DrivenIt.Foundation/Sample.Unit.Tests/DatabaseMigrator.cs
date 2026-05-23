using DrivenIt.Foundation.Data.TestTools;
using DrivenIt.Foundation.Infrastructure.Data;
using DrivenIt.Foundation.Sample.DataLayer;
using DrivenIt.Foundation.Sample.DataLayer.Migrations;
using System.Data.Entity.Migrations;

namespace Sample.Unit.Tests
{
    public class DatabaseMigrator : DatabaseBuilder
    {
        public DatabaseMigrator(ICreateDataContext dataContext, string host, string user, string password) : base(dataContext, host, user, password)
        {
        }

        public override void MigrateToLatestVersion()
        {
            var migrator = new DbMigrator(new Configuration());
            migrator.Update();
        }

        protected override void SetupDatabaseConnection(string connection)
        {
            DataContext.Connection = connection;
        }
    }
}
