using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Data;
using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Api.Services;

public sealed class ProjectQueryService(IDbContextFactory<ImportDbContext> dbContextFactory)
{
    public async Task<IReadOnlyList<ProjectSummary>> GetSummariesAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Projects
            .AsNoTracking()
            .OrderBy(project => project.Name)
            .Select(project => new ProjectSummary(project.Id, project.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectModel?> GetProjectAsync(int projectId, CancellationToken cancellationToken = default)
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
            .Include(component => component.Activities)
                .ThenInclude(activity => activity.OutgoingRelations)
            .ToListAsync(cancellationToken);

        var externalIds = await ExternalIdLoader.LoadByInternalIdAsync(db, cancellationToken);
        return ProjectEntityReader.ToModel(project, components, externalIds);
    }

    public async Task<ProjectImportPayload?> GetImportPayloadAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        var model = await GetProjectAsync(projectId, cancellationToken);
        return model is null ? null : ProjectEntityReader.ToImportPayload(model);
    }
}
