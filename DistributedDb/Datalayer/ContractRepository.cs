
using Domain.Contracts;

namespace Datalayer
{
    public class ContractRepository: IContractRepository
    {
        public void Add(Contract contract)
        {
            using (var cnt = new DataContext())
            {
                cnt.Contracts.Add(Map(contract));
                cnt.SaveChanges();
            }
        }

        private static Module.Two.Datalayer.Contract Map(Contract contract)
        {
            return new  Module.Two.Datalayer.Contract() {Id = contract.Id};
        }
    }
}
