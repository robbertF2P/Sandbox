using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Module.One.Contracts;
using Module.One.Models;
using Shared.Domain;

namespace Datalayer.One.Repositories
{
    public class ContractsRepository:IContractRepository
    {
        public ModuleContract GetById(Guid id)
        {
            using (var cnt = new ModuleOneContext())
            {
                var contract = cnt.Contracts.Single(c => c.Id == id);
                return MapTo(contract);
            }
        }

        private ModuleContract MapTo(DataLayer.Models.Contract contract)
        {
            return new ModuleContract();
        }
    }
}
