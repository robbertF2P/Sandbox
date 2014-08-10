using System;

namespace DrivenIt.Foundation.Domain.Tools.Events
{
    public static class ErrorEventListener
    {
        public static IDisposable Register(Action<GeneralErrorEvent> action)
        {
            return ErrorEventBroker.Current.Register(action);
        }
    }
}
