using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Threading.Tasks;
using DrivenIt.Foundation.Contracts;
using DrivenIt.Foundation.Contracts.UnitOfWork;

namespace DrivenIt.Foundation.Infrastructure.Data
{
    public class DataContextUoW : IUow
    {
        private readonly ISupportUow[] _supporters;
        private readonly DataContextHelper _contextWrapper;

        public DataContextUoW(IPrincipleContext principleContext, params ISupportUow[] supporters)
        {
            _supporters = supporters;
            _contextWrapper = new DataContextHelper(new DbContext(""), principleContext);
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
                    _contextWrapper.Context.Dispose();
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
