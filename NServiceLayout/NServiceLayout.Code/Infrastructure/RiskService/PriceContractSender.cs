using System;
using NServiceBus;
using NServiceLayout.InternalMessages.RiskService;

namespace NServiceLayout.RiskService
{
    public partial class PriceContractSender: IPriceContractSender, NServiceBus.INServiceBusComponent
    {
        public void Send(PriceContract message)
		{
			Bus.Send(message);	
		}

        public IBus Bus { get; set; }
    }

    public interface IPriceContractSender
    {
        void Send(PriceContract message);
    }
}
