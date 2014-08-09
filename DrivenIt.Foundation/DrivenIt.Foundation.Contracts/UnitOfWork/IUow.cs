using System;

namespace DrivenIt.Foundation.Contracts.UnitOfWork 
{
    public interface IUow:IDisposable
    {
        void SaveChanges();
        IDataContext GetContext();
    }
}
