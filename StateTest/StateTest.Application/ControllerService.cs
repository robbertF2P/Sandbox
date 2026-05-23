using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateTest.Domain;
using StateTest.Web.Models;

namespace StateTest.Application
{
    public class ControllerService
    {
        static ControllerService()
        {
            AutoMapper.Mapper.CreateMap<SomeDomainThing, SomeViewModel>()
                .AfterMap((s,d)=> d.State = Newtonsoft.Json.JsonConvert.SerializeObject(s));
            AutoMapper.Mapper.CreateMap<SubItem, SubItemViewModel>();
        }
        private readonly Repository _repository;

        public ControllerService():this(new Repository(new Uow()))
        {}
        public ControllerService(Repository repository)
        {
            _repository = repository;
        }
        public IEnumerable<SomeViewModel> LoadAll()
        {
            var items = _repository.GetAll();
            return AutoMapper.Mapper.Map<IEnumerable<SomeViewModel>>(items);
        }

        public SomeViewModel Edit(Guid id)
        {
            var items = _repository.GetById(id);

            return AutoMapper.Mapper.Map<SomeViewModel>(items);
        }

        public void Save(SomeViewModel viewModel)
        {
            var domainObject = Newtonsoft.Json.JsonConvert.DeserializeObject<SomeDomainThing>(viewModel.State);
            _repository.Update(domainObject);
        }

        public SomeViewModel Update(SomeViewModel viewModel)
        {
            var domainObject = Newtonsoft.Json.JsonConvert.DeserializeObject<SomeDomainThing>(viewModel.State);
            
            domainObject.ChangeName(viewModel.Name);
            return AutoMapper.Mapper.Map<SomeViewModel>(domainObject);

        }
    }
}
