using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Presentation;
using Contracts.Presentation.Models;

namespace Application.Layer
{
    public class QueryViewModelService : IQueryViewModels
    {
        private readonly IContractRepository _contractRepository;

        public QueryViewModelService(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository;
        }

        public ContractViewModel GetCurrentContract()
        {
            var contract = _contractRepository.GetCurrentContract();
            return Map(contract);
        }

        private ContractViewModel Map(Shared.Domain.Contract contract)
        {
            return new ContractViewModel();
        }
    }
}
