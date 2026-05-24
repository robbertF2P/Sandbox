using System;
using NServiceBus;
 
namespace NServiceLayout.Module.Two
{
	public partial class EndpointConfig : IConfigureThisEndpoint, AsA_Server, ISpecifyMessageHandlerOrdering    
	{
	    public void SpecifyOrder(Order order)
	    {
	        order.Specify(First<NServiceLayout.Module.Two.Infrastructure.Authentication>.Then<NServiceLayout.HedgeService.HedgeContractProcessor>());
	    }
    }
}
