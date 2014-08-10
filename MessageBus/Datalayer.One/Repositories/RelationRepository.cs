using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Module.One.Contracts;
using Module.One.Domain;
using Contract = Datalayer.One.Models.Contract;

namespace Datalayer.One.Repositories
{
    public class RelationRepository:IRelationRepository
    {
        public Relation GetById(Guid id)
        {
            using (var cnt = new ModuleOneContext())
            {
                var contract = cnt.Contracts.Single(c => c.Id == id);
                return MapTo(contract);
            }
        }

        private Relation MapTo(Contract contract)
        {
            return Relation.LoadRelation(Guid.Empty, "Test", new[] { MapTo(contract) });
        }
    }
}
