using System;
using System.Data.Entity;
using DrivenIt.Foundation.Contracts;
using DrivenIt.Foundation.Infrastructure.Data;

namespace DrivenIt.Foundation.Data.TestTools
{
    public abstract class DatabaseBuilder:IManageDatabase
    {
        private readonly ICreateDataContext _contextFactory;
        private DbContext _context;

        protected DatabaseBuilder(ICreateDataContext contextFactory , string host, string user, string password)
        {
            _contextFactory = contextFactory;

            Host = host;
            User = user;
            Password = password;
        }

        private const string DatabaseConnectionStringFormat = "data source={0};initial catalog=TEST_DB_{1};User={2};Password={3};integrated security=false;MultipleActiveResultSets=True;";

        public string Host { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }

        public void GenerateNewUniqueConnection()
        {
            CurrentConnection  = string.Format(DatabaseConnectionStringFormat,Host, DateTime.Now.Ticks, User, Password);
            SetupDatabaseConnection(CurrentConnection);
            _context = _contextFactory.Create();
        }

        public abstract void MigrateToLatestVersion();
        protected abstract void SetupDatabaseConnection(string connection);
        public string CurrentConnection { get; private set; }
        public string CurrentDatabase { get; private set; }
        public void ExecuteSqlCommand(string sqlCommand)
        {
            _context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, sqlCommand);
        }

    }
}