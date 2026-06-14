using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Contracts.Messages.Progress;

public sealed record PersistHourBookingResult(
    bool Success,
    HourBookingDto? Booking,
    int? ProjectId,
    string? ErrorMessage);
