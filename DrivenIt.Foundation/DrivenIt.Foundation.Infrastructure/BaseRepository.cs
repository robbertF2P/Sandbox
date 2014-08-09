using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrivenIt.Foundation.Contracts;
using DrivenIt.Foundation.Contracts.UnitOfWork;

namespace DrivenIt.Foundation.Infrastructure
{
    public abstract class BaseRepository :IRepository

    {
        private readonly IUow _defaultUnitOfWork;
        private IDataContext _context;
        protected IDataContext Context { get { return _context; } }

        protected BaseRepository(IUow unitOfWork)
        {
            _context = unitOfWork.GetContext();
            _defaultUnitOfWork = unitOfWork;
        }
        
        public void Set(IUow unitOfWork)
        {
            _context = unitOfWork.GetContext();
        }

        public void Reset()
        {
            _context = _defaultUnitOfWork.GetContext();
        }
    }
}
