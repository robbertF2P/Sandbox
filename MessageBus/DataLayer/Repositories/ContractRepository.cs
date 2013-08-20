using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Layer;
using Shared.Domain;

namespace DataLayer.Repositories
{
    public class ContractRepository: IContractRepository
    {
        public Contract GetCurrentContract()
        {
            using (var cnt = new FullContext())
            {
                var contract = cnt.Contracts.First();
                return MapTo(contract);
            }
        }

        private Contract MapTo(Models.Contract contract)
        {
            return new Contract()
                {

                };
        }
    }
}
