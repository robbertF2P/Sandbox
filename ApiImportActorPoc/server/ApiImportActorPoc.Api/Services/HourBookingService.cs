using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Values;
using ApiImportActorPoc.Data;
using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Api.Services;

public sealed class HourBookingService(IDbContextFactory<ImportDbContext> dbContextFactory)
{
    public async Task<IReadOnlyList<AssignmentListItem>> ListAssignmentsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var assignments = await db.Assignments
            .AsNoTracking()
            .Include(assignment => assignment.HourBookings)
            .Include(assignment => assignment.Activity)
                .ThenInclude(activity => activity.Component)
            .ToListAsync(cancellationToken);

        assignments = assignments
            .OrderBy(assignment => assignment.PersonName.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (assignments.Count == 0)
        {
            return [];
        }

        var projectIds = assignments
            .Select(assignment => assignment.Activity.Component.ProjectId)
            .Distinct()
            .ToList();

        var projects = await db.Projects
            .AsNoTracking()
            .Where(project => projectIds.Contains(project.Id))
            .ToDictionaryAsync(project => project.Id, cancellationToken);

        var components = await db.Components
            .AsNoTracking()
            .Where(component => projectIds.Contains(component.ProjectId))
            .ToListAsync(cancellationToken);

        return assignments
            .Select(assignment =>
            {
                var component = assignment.Activity.Component;
                var project = projects[component.ProjectId];
                var hoursWorked = assignment.HourBookings.Aggregate(Hours.Zero, (total, booking) => total + booking.Hours);

                return new AssignmentListItem(
                    assignment.Id,
                    project.Id,
                    project.Name,
                    BuildComponentPath(component, components),
                    assignment.Activity.Name,
                    assignment.PersonName,
                    assignment.BudgetedHours,
                    hoursWorked);
            })
            .ToList();
    }

    public async Task<HourBookingDto?> BookHoursAsync(
        int assignmentId,
        BookHoursRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Hours <= Hours.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Hours must be greater than zero.");
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var assignment = await db.Assignments
            .FirstOrDefaultAsync(entity => entity.Id == assignmentId, cancellationToken);

        if (assignment is null)
        {
            return null;
        }

        var booking = new HourBookingEntity
        {
            AssignmentId = assignmentId,
            Hours = request.Hours,
            BookedAt = DateTimeOffset.UtcNow,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        db.HourBookings.Add(booking);
        await db.SaveChangesAsync(cancellationToken);

        return new HourBookingDto(
            booking.Id,
            booking.AssignmentId,
            booking.Hours,
            booking.BookedAt,
            booking.Notes);
    }

    private static string BuildComponentPath(
        ComponentEntity component,
        IReadOnlyList<ComponentEntity> allComponents)
    {
        var segments = new List<string> { component.Name };
        var parentId = component.ParentComponentId;

        while (parentId is not null)
        {
            var parent = allComponents.Single(entity => entity.Id == parentId);
            segments.Insert(0, parent.Name);
            parentId = parent.ParentComponentId;
        }

        return string.Join(" / ", segments);
    }
}
