namespace DrivenIt.Foundation.Contracts.Events
{
    public interface IDomainEventReporter<in TEvent> where TEvent : IDomainEvent
    {
        void Raise(TEvent args);
    }
}