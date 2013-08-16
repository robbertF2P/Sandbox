using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messages.Module.One.Events;
using NServiceBus;

namespace Event.Aggragator
{
    public class EventReRaiser:IHandleMessages<NewContractCreated>
    {
        public IBus Bus { get; set; }

        public void Handle(IEvent message)
        {
            Console.WriteLine("Re-emitting event {0}", message.GetType().Name);
            Bus.Publish(message);
        }

        public void Handle(NewContractCreated message)
        {
            Console.WriteLine("Re-emitting event {0}", message.GetType().Name);
            Bus.Publish(message);
        }
    }
}
