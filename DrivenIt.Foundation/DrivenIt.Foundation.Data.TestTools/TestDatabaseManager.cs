using System;
using System.Data.Entity;
using DataFresh;
using DrivenIt.Foundation.Contracts;

namespace DrivenIt.Foundation.Data.TestTools
{
    public class TestDatabaseManager
    {
        private readonly IManageDatabase _databaseManager;

        public TestDatabaseManager(IManageDatabase databaseManager)
        {
            _databaseManager = databaseManager;
        }

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
            _databaseManager.GenerateNewUniqueConnection();
            _databaseManager.MigrateToLatestVersion();
            //DataContext.Connection = string.Format(DatabaseConnectionStringFormat, DateTime.Now.Ticks);
            //var migrator = new DbMigrator(new Configuration());
            //migrator.Update();
        }

        public void BeforeTest()
        {
            var connection = _databaseManager.CurrentConnection;
            _dataFresh = new SqlDataFresh(connection);

            if (_dataFreshInstalled) return;

            _databaseManager.ExecuteSqlCommand(EnableCmdShell);
            _dataFresh.PrepareDatabaseforDataFresh(true);
            _databaseManager.ExecuteSqlCommand(DisableCmdShell);
            _dataFreshInstalled = true;
        }

        public void AfterTest()
        {
            if (_dataFresh.HasDatabaseBeenModified())
                _dataFresh.RefreshTheDatabase();
        }

        public void RemoveDatabase()
        {
            _dataFreshInstalled = false;
            _databaseManager.ExecuteSqlCommand(String.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE ", _databaseManager.CurrentDatabase));
            _databaseManager.ExecuteSqlCommand(String.Format("USE master DROP DATABASE {0}", _databaseManager.CurrentDatabase));
        }
    }
}