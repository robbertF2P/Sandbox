using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.One.Domain
{
    public class Relation
    {
        public static Relation LoadRelation(Guid id, string name, IEnumerable<Contract> contracts)
        {
            return new Relation
                {
                    Id = Id,
                    Name = name,
                    PurchaseContracts = contracts
                };
        }

        private Relation()
        {}
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public IEnumerable<Contract> PurchaseContracts { get; private set; }
    }
}
