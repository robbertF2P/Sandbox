namespace F2pPlatform.Host.Contracts.Events;

public interface IActorSystemEvent;

public interface IDataEvent : IActorSystemEvent
{
    string? CorrelationId => null;
    string? UseCase => null;
}
