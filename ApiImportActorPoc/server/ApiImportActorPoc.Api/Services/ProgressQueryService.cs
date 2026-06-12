using ApiImportActorPoc.Contracts.Models.Progress;
using ApiImportActorPoc.Core.Progress;
using ApiImportActorPoc.Data;
using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Api.Services;

public sealed class ProgressQueryService(IDbContextFactory<ImportDbContext> dbContextFactory)
{
    public async Task<ProjectProgressDto?> GetProjectProgressAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == projectId, cancellationToken);

        if (project is null)
        {
            return null;
        }

        var components = await LoadComponentsWithHoursAsync(db, projectId, cancellationToken);
        return ProgressCalculator.ToProjectProgress(project, components);
    }

    private static async Task<List<ComponentEntity>> LoadComponentsWithHoursAsync(
        ImportDbContext db,
        int projectId,
        CancellationToken cancellationToken) =>
        await db.Components
            .AsNoTracking()
            .Where(component => component.ProjectId == projectId)
            .Include(component => component.Activities)
                .ThenInclude(activity => activity.Assignments)
                    .ThenInclude(assignment => assignment.HourBookings)
            .ToListAsync(cancellationToken);
}
