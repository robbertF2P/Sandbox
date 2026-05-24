using System;
using NServiceBus;
 
namespace NServiceLayout.Module.One
{
	public partial class EndpointConfig : IConfigureThisEndpoint, AsA_Publisher, ISpecifyMessageHandlerOrdering    
	{
	    public void SpecifyOrder(Order order)
	    {
	        order.Specify(First<NServiceLayout.Module.One.Infrastructure.Authentication>.Then<NServiceLayout.RiskService.PriceContractProcessor>());
	    }
    }
}
