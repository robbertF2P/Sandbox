using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Contracts.Events;

public sealed record ProgressRecalculated(
    Guid ProcessingId,
    int ProjectId,
    ProgressSummary Progress,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null,
    string? UseCase = null) : IDataEvent;
