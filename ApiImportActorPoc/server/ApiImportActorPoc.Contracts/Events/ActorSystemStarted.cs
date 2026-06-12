namespace ApiImportActorPoc.Contracts.Events;

public sealed record ActorSystemStarted(string SystemName, DateTimeOffset OccurredAt) : IActorSystemEvent;
