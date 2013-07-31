using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NSignaler.Contracts.UI.Commands;

namespace NSignaler.Processor
{
    public class UIHandler:IHandleMessages<DoSomething>
    {
        public void Handle(DoSomething message)
        {
            Console.WriteLine("WebSite: {0} heeft ons een bericht gestuurd.", message.Source);
            Console.WriteLine(message.Message);
        }
    }
}
