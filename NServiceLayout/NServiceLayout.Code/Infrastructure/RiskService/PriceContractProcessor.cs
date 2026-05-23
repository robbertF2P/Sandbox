using System;
using NServiceBus;
using NServiceLayout.InternalMessages.RiskService;


namespace NServiceLayout.RiskService
{
    public partial class PriceContractProcessor : IHandleMessages<PriceContract>
    {
		
		public void Handle(PriceContract message)
		{
			this.HandleImplementation(message);

			this.Bus.Publish<NServiceLayout.Contract.RiskService.NewContractPriced>(e => { /* set properties on e in here */ });
		}

		partial void HandleImplementation(PriceContract message);

		public IBus Bus { get; set; }

    }
}
