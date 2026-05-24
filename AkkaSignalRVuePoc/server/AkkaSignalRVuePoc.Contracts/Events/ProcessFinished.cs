namespace AkkaSignalRVuePoc.Contracts.Events;

public sealed record ProcessFinished(
    string ProcessId,
    DateTimeOffset FinishedAt) : IActorSystemEvent;
