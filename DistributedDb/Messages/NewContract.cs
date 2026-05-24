using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Contracts;
using NServiceBus;

namespace Messages
{
    public class NewContract:IMessage
    {
        public Contract Contract { get; set; }
    }
}
