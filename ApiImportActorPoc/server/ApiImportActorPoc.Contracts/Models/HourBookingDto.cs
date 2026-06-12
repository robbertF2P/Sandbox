using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Models;

public sealed record HourBookingDto(
    int Id,
    int AssignmentId,
    Hours Hours,
    DateTimeOffset BookedAt,
    string? Notes);
