using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;

namespace NSignaler.Contracts.Commands
{
    [TimeToBeReceived("00:00:10")]
    public class DisplayMessage:ICommand
    {
        public string Message { get; set; }
        public string UserSession { get; set; }
    }
}
