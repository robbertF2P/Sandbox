using System;
using System.Data.Entity;
using System.Linq;
using DrivenIt.Foundation.Contracts;
using DrivenIt.Foundation.Contracts.UnitOfWork;

namespace DrivenIt.Foundation.Infrastructure.Data
{
    internal class DataContextHelper : IDataContext
    {
        private readonly DbContext _context;
        private readonly IPrincipleContext _principleContext;

        internal DataContextHelper(DbContext context, IPrincipleContext principleContext)
        {
            _context = context;
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
            return _context.Set<TEntity>();
        }
        
        public void Update<TEntity>(TEntity entity) where TEntity : class 
        {
            var entry = _context.Entry(entity);
        }

        public TEntity Add<TEntity>(TEntity entity) where TEntity : class
        {
            var set = Set<TEntity>();
            set.Add(entity);
            var entry = _context.Entry(entity);
            return entry.Entity;
        }


        public TEntity Remove<TEntity>(TEntity entity) where TEntity : class
        {
            var set = Set<TEntity>();
            return set.Remove(entity);
        }

        public int ExecuteSqlCommand(string sqlCommand, object[] paramsObjects)
        {
            return _context.Database.ExecuteSqlCommand(sqlCommand, paramsObjects);
        }

        public int ExecuteSqlCommand(string sqlCommand)
        {
            return _context.Database.ExecuteSqlCommand(sqlCommand);
        }

        public void SaveChanges()
        {
            foreach (var entry in _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
            {
                if (entry.Entity is ISupportAudit)
                {
                    var auditEntity = entry.Entity as ISupportAudit;
                    auditEntity.CreatedBy = _principleContext.IsAnonymous? "anonymous" :  _principleContext.Name;
                    auditEntity.CreatedOn = DateTime.Now;
                }
            }
            foreach (var entry in _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified))
            {
                if (entry.Entity is ISupportAudit)
                {
                    var auditEntity = entry.Entity as ISupportAudit;
                    auditEntity.ModifiedBy = _principleContext.IsAnonymous ? "anonymous" : _principleContext.Name;
                    auditEntity.ModifiedOn = DateTime.Now;
                }
            }
            _context.SaveChanges();
        }

        internal void Dispose()
        {
            _context.Dispose();
        }
    }
}