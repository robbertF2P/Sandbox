using System.Security.Cryptography.X509Certificates;
using DrivenIt.Foundation.Contracts;

namespace DrivenIt.Foundation.Sample.Contracts
{
    public interface ISampleRepository:IRepository
    {
        void CreateNew(CreateTheThing task);
        SomeDomainThing GetItByName(string name);
    }
}