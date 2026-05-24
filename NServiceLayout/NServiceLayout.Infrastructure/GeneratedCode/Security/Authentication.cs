using System;
using NServiceBus;

namespace NServiceLayout.Infrastructure.Security
{
    public partial class Authentication //: IHandleMessages<object>
    {
        public void Handle(object message)
        {
            this.HandleImplementation(message);
        }

        public IBus Bus { get; set; }
    }
}
