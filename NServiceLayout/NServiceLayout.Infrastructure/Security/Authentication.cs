using System;
using NServiceBus;

namespace NServiceLayout.Infrastructure.Security
{
    public partial class Authentication
    {
        public virtual void HandleImplementation(object message)
        {
            // Implement your custom logic as needed
            // This logic will be applied to all the endpoints
            // that doesn't customize authentication
			// overriding it.
            if (this.Bus.GetMessageHeader(message, "User") == null)
            {
                //this.Bus.DoNotContinueDispatchingCurrentMessageToHandlers();
            }
        }
    }
}
