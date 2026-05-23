using System;
using System.Linq;
using ContextTests.Contracts;

namespace ContextTests.Dal
{
    public class DbWorkSpace:IWorkSpace
    {
        private DataContext _context;

        public DbWorkSpace()
        {
            _context = new DataContext();
        }
        public void PersistAll()
        {
            _context.SaveChanges();
        }

        public IQueryable<T> GetQueryable<T>() where T : class
        {
            return _context.Set<T>();
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Set<T>().Add(entity);
        }

        #region dispose
        private bool _disposed = false;
        
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}