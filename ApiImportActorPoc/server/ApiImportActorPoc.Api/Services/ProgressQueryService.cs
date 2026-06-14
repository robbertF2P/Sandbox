using ApiImportActorPoc.Contracts.Models.Progress;
using ApiImportActorPoc.Core.Progress;

namespace ApiImportActorPoc.Api.Services;

public sealed class ProgressQueryService(ProjectProgressLoader progressLoader)
{
    public Task<ProjectProgressDto?> GetProjectProgressAsync(
        int projectId,
        CancellationToken cancellationToken = default) =>
        progressLoader.LoadAsync(projectId, cancellationToken);
}
