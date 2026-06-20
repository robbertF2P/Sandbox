using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Contracts.Events;

public sealed record ImportCompleted(
    Guid SessionId,
    ProjectModel Model,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null,
    string? UseCase = null) : IDataEvent;
