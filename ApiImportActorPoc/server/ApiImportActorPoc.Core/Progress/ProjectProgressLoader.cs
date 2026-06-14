using ApiImportActorPoc.Contracts.Models.Progress;
using ApiImportActorPoc.Data;
using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Progress;

public sealed class ProjectProgressLoader(IDbContextFactory<ImportDbContext> dbContextFactory)
{
    public async Task<ProjectProgressDto?> LoadAsync(
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

        var components = await db.Components
            .AsNoTracking()
            .Where(component => component.ProjectId == projectId)
            .Include(component => component.Activities)
                .ThenInclude(activity => activity.Assignments)
                    .ThenInclude(assignment => assignment.HourBookings)
            .ToListAsync(cancellationToken);

        return ProgressCalculator.ToProjectProgress(project, components);
    }
}
