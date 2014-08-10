using System;
using System.Collections.Generic;
using DrivenIt.Foundation.Contracts;
using DrivenIt.Foundation.Contracts.Events;

namespace DrivenIt.Foundation.Domain.Tools.Events
{
    public class DomainEventBroker<TEvent>:IDomainEventListener<TEvent>, IDomainEventReporter<TEvent> where TEvent : IDomainEvent
    {
        [ThreadStatic]
        private static List<Action<TEvent>> _actions;

        private bool _raisedEvents;

        protected List<Action<TEvent>> Actions
        {
            get
            {
                if (_actions == null)
                    _actions = new List<Action<TEvent>>();

                return _actions;
            }
        }

        public IDisposable Register(Action<TEvent> callback)
        {
            Actions.Add(callback);
            ResetFlag();
            return new DomainEventRegistrationRemover(() => Actions.Remove(callback));
        }

        public virtual void Raise(TEvent args)
        {
            _raisedEvents = true;
            
            foreach (var action in Actions)
            {
                action(args);
            }
        }

        public bool RaisedEvents
        {
            get { return _raisedEvents; }
        }

        public void ResetFlag()
        {
            _raisedEvents = false;
        }
    }
}