namespace ApiImportActorPoc.Contracts.Models;

public sealed record HourBookingDto(
    int Id,
    int AssignmentId,
    decimal Hours,
    DateTimeOffset BookedAt,
    string? Notes);
