using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Values;
using ApiImportActorPoc.Data;
using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Progress;

public sealed class HourBookingPersistService(IDbContextFactory<ImportDbContext> dbContextFactory)
{
    public async Task<PersistHourBookingOutcome> PersistAsync(
        int assignmentId,
        Hours hours,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        if (hours <= Hours.Zero)
        {
            return new PersistHourBookingOutcome(false, null, null, "Hours must be greater than zero.");
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var assignment = await db.Assignments
            .Include(entity => entity.Activity)
                .ThenInclude(activity => activity.Component)
            .FirstOrDefaultAsync(entity => entity.Id == assignmentId, cancellationToken);

        if (assignment is null)
        {
            return new PersistHourBookingOutcome(false, null, null, "Assignment not found.");
        }

        var booking = new HourBookingEntity
        {
            AssignmentId = assignmentId,
            Hours = hours,
            BookedAt = DateTimeOffset.UtcNow,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };

        db.HourBookings.Add(booking);
        await db.SaveChangesAsync(cancellationToken);

        var projectId = assignment.Activity.Component.ProjectId;
        var bookingDto = new HourBookingDto(
            booking.Id,
            booking.AssignmentId,
            booking.Hours,
            booking.BookedAt,
            booking.Notes);

        return new PersistHourBookingOutcome(true, bookingDto, projectId, null);
    }
}

public sealed record PersistHourBookingOutcome(
    bool Success,
    HourBookingDto? Booking,
    int? ProjectId,
    string? ErrorMessage);
