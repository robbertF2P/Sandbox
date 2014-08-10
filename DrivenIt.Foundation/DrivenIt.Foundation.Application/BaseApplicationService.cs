using System;
using DrivenIt.Foundation.Contracts;
using DrivenIt.Foundation.Contracts.UnitOfWork;
using DrivenIt.Foundation.Domain.Tools.Automapper;
using DrivenIt.Foundation.Domain.Tools.Events;
using Seterlund.CodeGuard;

namespace DrivenIt.Foundation.Application
{
    public abstract class BaseApplicationService:IApplicationService
    {
        public IUowFactory UowFactory { get; set; } //dependancy property
        private IRepository repository;
        public void Test()
        {
            using(ErrorEventListener.Register(e=> Console.WriteLine("{0} : {1}",e.Key,e.Message)))
            using (var uow = StartUow(repository))
            {
                IViewModel model= null;
                var task=  model.ToTask<IDomainTask>()>()
                ;
                repository.Process(null);
                ErrorEventReporter.Raise(new GeneralErrorEvent("This doesn't work"));
                uow.SaveChanges();
            }
        }

        private IUow StartUow(params ISupportUow[] supporters)
        {
            Guard.That(supporters).IsNotEmpty();

            return UowFactory.StartUnitOfWork(supporters);
        }
    }
}
