using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Contracts.Messages.Progress;

public sealed record BookHoursResult(
    bool Success,
    HourBookingDto? Booking,
    string? ErrorMessage);
