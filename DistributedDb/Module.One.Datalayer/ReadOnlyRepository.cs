using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Module.One.Domain.Contract;
using Module.One.Domain.Contracts;

namespace Module.One.Datalayer
{
    public class ReadOnlyRepository
    {
        public Contract GetById()
        {
            using (var cnt = new ModeluOneContext())
            {
                return Map(cnt.Contracts.First());
            }
        }

        private Contract Map(ModuleOneContract first)
        {
            return new Contract
                {
                    Id = first.Id,
                    Name = first.Name
                };
        }
    }
}
