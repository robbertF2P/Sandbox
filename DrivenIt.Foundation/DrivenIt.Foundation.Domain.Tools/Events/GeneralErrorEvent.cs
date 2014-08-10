using DrivenIt.Foundation.Contracts;
using DrivenIt.Foundation.Contracts.Events;

namespace DrivenIt.Foundation.Domain.Tools.Events
{
    public class GeneralErrorEvent : IDomainEvent
    {
        public GeneralErrorEvent(string key, string message)
        {
            Key = key;
            Message = message;
        }

        public GeneralErrorEvent(string message)
        {
            Message = message;
        }

        public string Key { get; private set; }
        public string Message { get; private set; }
    }
}