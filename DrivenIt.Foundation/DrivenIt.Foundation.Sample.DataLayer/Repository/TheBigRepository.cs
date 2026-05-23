using DrivenIt.Foundation.Contracts.UnitOfWork;
using DrivenIt.Foundation.Infrastructure;
using DrivenIt.Foundation.Infrastructure.Data.Automapper;
using DrivenIt.Foundation.Sample.Contracts;
using DrivenIt.Foundation.Sample.DataLayer.Models;
using System.Linq;

namespace DrivenIt.Foundation.Sample.DataLayer.Repository
{
    public class TheBigRepository:BaseRepository,ISampleRepository
    {
        public TheBigRepository(IUow unitOfWork) : base(unitOfWork)
        {
        }

        public void CreateNew(CreateTheThing task)
        {
            var entity = task.To<SomeBitOfData>();
            Context.Add(entity);

        }

        public SomeDomainThing GetItByName(string name)
        {
            var model = base.Context.GetSet<SomeBitOfData>().SingleOrDefault(s => s.MyName == name);
            return model.To<SomeDomainThing>();
        }
    }
}
