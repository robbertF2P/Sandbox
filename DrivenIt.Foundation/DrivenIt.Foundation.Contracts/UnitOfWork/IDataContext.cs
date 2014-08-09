using System.Linq;

namespace DrivenIt.Foundation.Contracts.UnitOfWork 
{
    public interface IDataContext
    {
        bool IsReadOnly { get; }
        IQueryable<TEntity> GetSet<TEntity>() where TEntity : class;

        ///// <summary>
        ///// Use this if you want the domain entity to get updated with the generated Id on savechanges
        ///// </summary>
        //TEntity Add<TEntity, TDomain>(TEntity entity, TDomain domainEntity)
        //    where TEntity : class
        //    where TDomain : IDomainModel;

        TEntity Add<TEntity>(TEntity entity)
            where TEntity : class;
        
        void Update<TEntity>(TEntity entity)
            where TEntity : class;
        TEntity Remove<TEntity>(TEntity entity) where TEntity : class;
        int ExecuteSqlCommand(string sqlCommand, object[] paramsObjects);
        int ExecuteSqlCommand(string sqlCommand);
    }
}