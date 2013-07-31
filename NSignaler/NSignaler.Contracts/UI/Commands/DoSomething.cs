using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;

namespace NSignaler.Contracts.UI.Commands
{
    public class DoSomething:ICommand
    {
        public string Source { get; set; }
        public string Message { get; set; }
    }
}
