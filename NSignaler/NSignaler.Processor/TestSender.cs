using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NSignaler.Contracts.Commands;

namespace NSignaler.Processor
{
    public class TestSender:IWantToRunAtStartup
    {
        public IBus Bus { get; set; }
        public void Run()
        {
            Bus.Send("NSignaler.WebOne", new DisplayMessage(){Message = "Hallo daar", UserSession = "Groupje"});
        }

        public void Stop()
        {
        }
    }
}
