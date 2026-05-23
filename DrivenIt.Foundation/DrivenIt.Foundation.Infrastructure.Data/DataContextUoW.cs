using DrivenIt.Foundation.Contracts;
using DrivenIt.Foundation.Contracts.UnitOfWork;
using System;
using System.Data.Entity;

namespace DrivenIt.Foundation.Infrastructure.Data
{
    public class DataContextUow : IUow
    {
        public static Func<DbContext> ContextFactory; 
        private readonly ISupportUow[] _supporters;
        private readonly DataContextHelper _contextWrapper;

        public DataContextUow(IPrincipleContext principleContext, params ISupportUow[] supporters)
        {
            _supporters = supporters;
            _contextWrapper = new DataContextHelper(ContextFactory(), principleContext);
        }

        public void SaveChanges()
        {
            _contextWrapper.SaveChanges();
        }

        public IDataContext GetContext()
        {
            return _contextWrapper;
        }

        private void RestoreDefaultUnitOfWork()
        {
            foreach (var sup in _supporters)
            {
                sup.Reset();
            }
        }

        #region dispose

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _contextWrapper.Dispose();
                    RestoreDefaultUnitOfWork();
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
