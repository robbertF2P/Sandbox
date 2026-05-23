using System;
using NServiceBus;
 
namespace NServiceLayout.Module.One
{
	public class TransportConfig : INeedInitialization
	{
		public void Init()
		{
			// Tranport: Msmq (Default) - No configuration needed
		}
	}
}
