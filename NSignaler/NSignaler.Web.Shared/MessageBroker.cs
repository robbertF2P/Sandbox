using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NSignaler.Contracts.Commands;

namespace NSignaler.Web.Shared
{
    public class MessageBroker:IHandleMessages<IMessage>
    {
        private readonly IBus _bus;

        public MessageBroker(IBus bus)
        {
            _bus = bus;
        }

        public void SendCommand(ICommand command, string sessionId)
        {
            _bus.Send("NSignaler.Processor",command);
            Notify("Sending command now", sessionId);
        }

        private void Notify(string message, string sessionId)
        {
            _bus.SendLocal(new DisplayMessage { Message = message, UserSession = sessionId });
        }

        public void Handle(IMessage message)
        {
            if(message.)
        }
    }
}
