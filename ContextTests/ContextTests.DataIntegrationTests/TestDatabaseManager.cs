using System;
using System.Data.Entity.Migrations;
using ContextTests.Dal;
using ContextTests.Dal.Migrations;
using DataFresh;

namespace ContextTests.DataIntegrationTests
{
    public class TestDatabaseManager
    {
        // set this to the correct sql target site
        private const string DatabaseConnectionStringFormat = "data source=sqlserver;initial catalog=TEST_DB_{0};User=testUser;Password=test1234;integrated security=false;MultipleActiveResultSets=True;";

        private static bool _dataFreshInstalled = false;
        private SqlDataFresh _dataFresh;


        private const string EnableCmdShell = @"
EXEC sp_configure 'show advanced options', 1
RECONFIGURE
EXEC sp_configure 'xp_cmdshell', 1
RECONFIGURE";
        private const string DisableCmdShell = @"
EXEC sp_configure 'xp_cmdshell', 0
RECONFIGURE";

        public void SetupDatabase()
        {
            DataContext.Connection = string.Format(DatabaseConnectionStringFormat, DateTime.Now.Ticks);
            var migrator = new DbMigrator(new Configuration());
            migrator.Update();
        }

        public void BeforeTest()
        {
            var context = new DataContext();
            _dataFresh = new SqlDataFresh(DataContext.Connection);

            if (_dataFreshInstalled) return;

            context.Database.ExecuteSqlCommand(EnableCmdShell);
            _dataFresh.PrepareDatabaseforDataFresh(true);
            context.Database.ExecuteSqlCommand(DisableCmdShell);
            _dataFreshInstalled = true;
        }

        public void AfterTest()
        {
            if (_dataFresh.HasDatabaseBeenModified())
                _dataFresh.RefreshTheDatabase();
        }

        public void RemoveDatabase()
        {
            var context = new DataContext();
            context.Database.ExecuteSqlCommand(String.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", context.Database.Connection.Database));
            context.Database.ExecuteSqlCommand(String.Format("USE master DROP DATABASE {0}", context.Database.Connection.Database));
        }
    }
}