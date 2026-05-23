namespace DrivenIt.Foundation.Contracts
{
    public interface IManageDatabase
    {
        void GenerateNewUniqueConnection();
        void MigrateToLatestVersion();
        string CurrentConnection { get; }
        string CurrentDatabase { get; }
        void ExecuteSqlCommand(string sqlCommand);
    }
}