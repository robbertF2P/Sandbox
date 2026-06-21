namespace AkkaSignalRVuePoc.Contracts.Events;

public interface IDataEvent : IActorSystemEvent
{
    string? CorrelationId => null;
    string? UseCase => null;
}
