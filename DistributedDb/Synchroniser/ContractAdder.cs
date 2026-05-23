using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datalayer;
using Domain.Contracts;
using Messages;
using NServiceBus;

namespace Synchroniser
{
    public class ContractAdder:IHandleMessages<NewContract>
    {
        private readonly IContractRepository _contractRepository;

        public ContractAdder():this(new ContractRepository())
        {
        }

        public ContractAdder(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository;
        }

        public void Handle(NewContract message)
        {
            _contractRepository.Add(message.Contract);
        }
    }
}
