using System;
using System.Collections.Generic;
using DrivenIt.Foundation.Contracts;
using DrivenIt.Foundation.Contracts.UnitOfWork;

namespace DrivenIt.Foundation.Infrastructure.Data
{
    public class UnitOfWorkFactory : IUowFactory
    {
        private readonly IPrincipleContext _principleContext;
        
        public UnitOfWorkFactory(IPrincipleContext  principleContext)
        {
            _principleContext = principleContext;
        }

        public IUow StartUnitOfWork(params ISupportUow[] supporters)
        {
            if (supporters.Length == 0) throw new ArgumentException("at least one supporter should have been specified");
            var unitOfWork = new DataContextUow(_principleContext, supporters);
            SetSupporters(supporters, unitOfWork);
            return unitOfWork;
        }

        private static void SetSupporters(IEnumerable<ISupportUow> unitOfWorkSupporters, IUow unitOfWork)
        {
            foreach (var supporter in unitOfWorkSupporters)
            {
                supporter.Set(unitOfWork);
            }
        }
    }
}