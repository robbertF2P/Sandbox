using System;
using NServiceBus;
 
namespace NServiceLayout.MySite
{
	public class TransportConfig : INeedInitialization
	{
		public void Init()
		{
			// Tranport: Msmq (Default) - No configuration needed
  		}
	}
}
