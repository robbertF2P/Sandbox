namespace DrivenIt.Foundation.Domain.Tools.Events
{
    public static class ErrorEventReporter
    {
        public static void Raise(GeneralErrorEvent @event)
        {
            ErrorEventBroker.Current.Raise(@event);
        }
    }
}