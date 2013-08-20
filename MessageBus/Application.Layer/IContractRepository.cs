using Shared.Domain;

namespace Application.Layer
{
    public interface IContractRepository
    {
        Contract GetCurrentContract();
    }
}