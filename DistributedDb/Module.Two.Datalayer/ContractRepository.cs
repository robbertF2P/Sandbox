using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.Two.Datalayer
{
    public class ContractRepository
    {
        public void Add(Domain.Contracts.Contract contract)
        {
            using (var cnt = new ModeluTwoContext())
            {
                cnt.Contracts.Add(Map(contract));
            }
        }

        private Contract Map(Domain.Contracts.Contract contract)
        {
            return new Contract
                {
                    Id = contract.Id,
                    Name = "woei",
                    SomeProperty = true,
                };
        }
    }
}
