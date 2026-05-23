using AutoMapper;
using DrivenIt.Foundation.Contracts;
using DrivenIt.Foundation.Sample.Contracts;
using DrivenIt.Foundation.Sample.DataLayer.Models;

namespace DrivenIt.Foundation.Sample.DataLayer.Automapper
{
    public class DataProfile:Profile, IWantToRunOnStartup
    {
        protected override void Configure()
        {
            Mapper.CreateMap<SomeBitOfData, SomeDomainThing>();
            Mapper.CreateMap<CreateTheThing, SomeBitOfData>();
        }

        public void Run()
        {
            Mapper.AddProfile<DataProfile>();
        }
    }
}
