namespace AkkaSignalRVuePoc.Contracts.Events;

public sealed record ActorSystemStarted(
    string ActorSystemName,
    DateTimeOffset StartedAt) : IActorSystemEvent;
