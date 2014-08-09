using System;
using System.Data.Entity;
using System.Linq;
using DrivenIt.Foundation.Contracts;
using DrivenIt.Foundation.Contracts.UnitOfWork;

namespace DrivenIt.Foundation.Infrastructure.Data
{
    internal class DataContextHelper : IDataContext
    {
        internal DbContext Context;
        
        //private readonly List<Tuple< IDataModel, IDomainModel>> _trackData = new List<Tuple< IDataModel, IDomainModel>>();
        private readonly IPrincipleContext _principleContext;

        internal DataContextHelper(DbContext context, IPrincipleContext principleContext)
        {
            Context = context;
            IsReadOnly = principleContext == null;
            _principleContext = principleContext;
        }

        public bool IsReadOnly { get; private set; }
        public IQueryable<TEntity> GetSet<TEntity>() where TEntity : class
        {
            return Set<TEntity>();
        }

        private IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return Context.Set<TEntity>();
        }

        //public TEntity Add<TEntity,TDomain>(TEntity entity, TDomain domainEntity) 
        //    where TEntity : class
        //    where TDomain:IDomainModel
        //{
        //    var set = Set<TEntity>();
        //    var tuple = new Tuple< IDataModel, IDomainModel>(( IDataModel) entity, domainEntity);
        //    _trackData.Add(tuple);
        //    return set.Add(entity);
        //}

        public void Update<TEntity>(TEntity entity) where TEntity : class 
        {
            var entry = Context.Entry(entity);
        }

        public TEntity Add<TEntity>(TEntity entity) where TEntity : class
        {
            var set = Set<TEntity>();
            set.Add(entity);
            var entry = Context.Entry(entity);
            return entry.Entity;
        }


        public TEntity Remove<TEntity>(TEntity entity) where TEntity : class
        {
            var set = Set<TEntity>();
            return set.Remove(entity);
        }

        public int ExecuteSqlCommand(string sqlCommand, object[] paramsObjects)
        {
            return Context.Database.ExecuteSqlCommand(sqlCommand, paramsObjects);
        }

        public int ExecuteSqlCommand(string sqlCommand)
        {
            return Context.Database.ExecuteSqlCommand(sqlCommand);
        }

        public void SaveChanges()
        {
            foreach (var entry in Context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
            {
                if (entry.Entity is ISupportAudit)
                {
                    var auditEntity = entry.Entity as ISupportAudit;
                    auditEntity.CreatedBy = _principleContext.IsAnonymous? "system" :  _principleContext.Name;
                    auditEntity.CreatedOn = DateTime.Now;
                }
            }
            foreach (var entry in Context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified))
            {
                if (entry.Entity is ISupportAudit)
                {
                    var auditEntity = entry.Entity as ISupportAudit;
                    auditEntity.ModifiedBy = _principleContext.IsAnonymous ? "system" : _principleContext.Name;
                    auditEntity.ModifiedOn = DateTime.Now;
                }
            }
            Context.SaveChanges();
            //ReportBackIds();
        }

        //protected void ReportBackIds()
        //{
        //    foreach (var tu in _trackData)
        //    {
        //        tu.Item2.Id = tu.Item1.Id;
        //    }
        //    _trackData.Clear();
        //}
    }
}