using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using NServiceBus;
using NSignaler.Contracts.Commands;

namespace NSignaler.Web.Shared
{
    public class MessageDisplayer:IHandleMessages<DisplayMessage>
    {
        public void Handle(DisplayMessage message)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<EventHub>();
            //hub.Clients.All.DisplayMessage(message.Message);
            hub.Clients.Group(message.UserSession).DisplayMessage(message.Message);
        }
    }
}
