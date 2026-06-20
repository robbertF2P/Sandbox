using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Contracts.Events;

public sealed record HoursBooked(
    Guid ProcessingId,
    HourBookingDto Booking,
    int ProjectId,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null,
    string? UseCase = null) : IDataEvent;
