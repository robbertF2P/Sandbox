namespace AkkaSignalRVuePoc.Contracts.Events;

public sealed record ProcessFinished(
    string ProcessId,
    DateTimeOffset FinishedAt,
    string? CorrelationId = null,
    string? UseCase = null) : IActorSystemEvent;
