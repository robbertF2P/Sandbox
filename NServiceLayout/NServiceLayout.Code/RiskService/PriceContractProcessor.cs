using System;
using NServiceBus;
using NServiceLayout.InternalMessages.RiskService;


namespace NServiceLayout.RiskService
{
    public partial class PriceContractProcessor
    {
		
        partial void HandleImplementation(PriceContract message)
        {
            // Implement your handler logic here.
            Console.WriteLine("RiskService received " + message.GetType().Name);
        }

            // call Bus.Publish<NServiceLayout.Contract.RiskService.NewContractPriced>(m => { /* set properties on m in here */ });

    }
}
