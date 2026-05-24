using AkkaSignalRVuePoc.Contracts.Data;
using AkkaSignalRVuePoc.Data.Entities;

namespace AkkaSignalRVuePoc.Core.Data;

internal static class CatalogEntityMapper
{
    public static OrganisationDto ToDto(Organisation organisation) =>
        new(organisation.Id, organisation.Name, organisation.CreatedAt);

    public static ProjectDto ToDto(Project project) =>
        new(
            project.Id,
            project.OrganisationId,
            project.Name,
            project.Description,
            project.CreatedAt);
}
