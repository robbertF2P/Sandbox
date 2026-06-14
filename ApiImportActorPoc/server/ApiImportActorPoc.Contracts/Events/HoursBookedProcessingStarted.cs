using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Events;

public sealed record HoursBookedProcessingStarted(
    Guid ProcessingId,
    int AssignmentId,
    Hours Hours,
    DateTimeOffset OccurredAt) : IDataEvent;
