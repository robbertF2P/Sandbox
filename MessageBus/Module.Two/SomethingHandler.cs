using System;
using Messages.Module.One.Events;
using NServiceBus;

namespace Module.Two
{
    public class SomethingHandler:IHandleMessages<NewContractCreated>
    {
        public void Handle(NewContractCreated message)
        {
            Console.WriteLine("A new contract was created!");
        }
    }
}
