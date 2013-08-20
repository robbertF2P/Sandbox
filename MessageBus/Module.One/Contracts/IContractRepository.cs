using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Module.One.Models;
using Shared.Domain;

namespace Module.One.Contracts
{
    public interface IContractRepository
    {
        ModuleContract GetById(Guid id);
    }
}
