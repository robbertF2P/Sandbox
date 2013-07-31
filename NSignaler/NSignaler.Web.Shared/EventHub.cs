using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NServiceBus;

namespace NSignaler.Web.Shared
{
    public class EventHub:Hub
    {
        private readonly IBus _bus;

        public EventHub(IBus bus)
        {
            _bus = bus;
        }

        public void Init(string sessionId)
        {
            SendMessage("You are connected");
            Groups.Add(Context.ConnectionId, "Groupje");
        }

        public void SendMessage(string msg)
        {
            Clients.Caller.DisplayMessage(msg);
        }
    }
}
