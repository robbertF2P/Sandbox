using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Module.One.Domain;
using Shared.Domain;

namespace Module.One.Contracts
{
    public interface IRelationRepository
    {
        Relation GetById(Guid id);
    }

}
