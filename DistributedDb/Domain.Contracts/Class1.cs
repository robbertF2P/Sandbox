using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public class Contract
    {
        public Guid Id { get; set; }
    }

    public interface IContractRepository
    {
        void Add(Contract contract);
    }
}
