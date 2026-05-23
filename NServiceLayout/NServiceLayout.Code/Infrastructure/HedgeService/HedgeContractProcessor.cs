using System;
using NServiceBus;
using NServiceLayout.Contract.RiskService;


namespace NServiceLayout.HedgeService
{
    public partial class HedgeContractProcessor : IHandleMessages<NewContractPriced>
    {
		
		public void Handle(NewContractPriced message)
		{
			this.HandleImplementation(message);
		}

		partial void HandleImplementation(NewContractPriced message);

		public IBus Bus { get; set; }

    }
}
