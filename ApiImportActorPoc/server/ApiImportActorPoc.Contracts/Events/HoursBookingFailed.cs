namespace ApiImportActorPoc.Contracts.Events;

public sealed record HoursBookingFailed(
    Guid ProcessingId,
    int AssignmentId,
    string ErrorMessage,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null,
    string? UseCase = null) : IDataEvent;
