using System;
using Messages.Module.One.Commands;
using Messages.Module.One.Events;
using NServiceBus;

namespace Module.One
{
    public class ContractCreator:IHandleMessages<CreateContract>
    {
        public IBus Bus { get; set; }

        public void Handle(CreateContract message)
        {
            Console.WriteLine("Creating new contract");
            Bus.Publish(new NewContractCreated());
        }
    }
}
