using System;

namespace DrivenIt.Foundation.Contracts.Events
{
    public interface IDomainEventListener<out TEvent> where TEvent : IDomainEvent
    {
        IDisposable Register(Action<TEvent> callback);
        bool RaisedEvents { get; }
    }
}