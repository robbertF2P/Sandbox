using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrivenIt.Foundation.Contracts
{
    public interface IDomainModel
    {
    }

    
    public interface IPrincipleContext
    {
        // some details about the current principle
        bool IsAnonymous { get; }

        string Name { get; }
    }
}
