using System;
using System.Threading;

namespace DrivenIt.Foundation.Domain.Tools.Events
{
    internal class ErrorEventBroker : DomainEventBroker<GeneralErrorEvent>
    {
        private static readonly Lazy<ErrorEventBroker> Lazy =
            new Lazy<ErrorEventBroker>(() => new ErrorEventBroker(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static ErrorEventBroker Current
        {
            get { return Lazy.Value; }
        }

        public static bool ErrorsWereRaised
        {
            get { return Current.RaisedEvents; }
        }

        public static void ResetRaisedFlag()
        {
            Current.ResetFlag();
        }

        
    }
}