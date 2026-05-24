using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateTest.Domain
{
    public class Uow
    {
        static Uow()
        {
            var itemOne = new SomeDomainThing("mything");
            itemOne.AddNewItem("some", 45.6m);
            itemOne.AddNewItem("hoi",0);
            var itemTwo = new SomeDomainThing("anothr");
            data.Add(itemOne);
            data.Add(itemTwo);
        }

        private static List<SomeDomainThing> data = new List<SomeDomainThing>();
        public IQueryable<SomeDomainThing> GetSet()
        {
            return data.AsQueryable();
        }

        public void Remove(SomeDomainThing stored)
        {
            data.Remove(stored);
        }

        public void Store(SomeDomainThing someDomainThing)
        {
            data.Add(someDomainThing);
        }
    }
    public class Repository
    {
        private readonly Uow _uow;

        public Repository(Uow uow)
        {
            _uow = uow;
        }

        public SomeDomainThing GetById(Guid id)
        {
            return _uow.GetSet().SingleOrDefault(s => s.Id == id);
        }

        public IEnumerable<SomeDomainThing> GetAll()
        {
            return _uow.GetSet().AsEnumerable();
        }

        public void Update(SomeDomainThing someDomainThing)
        {
            var stored = GetById(someDomainThing.Id);
            _uow.Remove(stored);
            _uow.Store(someDomainThing);
        }
    }
}
