using System;
using System.Linq;

namespace ContextTests.Contracts
{
    public interface IWorkSpace : IDisposable 
    {
        void PersistAll();
        IQueryable<T> GetQueryable<T>() where T : class;
        void Add<T>(T entity) where T : class;
    }
}