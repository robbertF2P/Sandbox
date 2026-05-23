using System;
using NServiceBus;
using NServiceLayout.Contract.RiskService;


namespace NServiceLayout.HedgeService
{
    public partial class HedgeContractProcessor
    {
		
        partial void HandleImplementation(NewContractPriced message)
        {
            // Implement your handler logic here.
            Console.WriteLine("HedgeService received " + message.GetType().Name);
        }

    }
}
