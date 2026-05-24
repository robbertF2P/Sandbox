using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Api.Models;

internal static class CatalogModelMapper
{
    public static Organisation ToApiModel(OrganisationDto dto) =>
        new(dto.Id, dto.Name, dto.CreatedAt);

    public static Project ToApiModel(ProjectDto dto) =>
        new(dto.Id, dto.OrganisationId, dto.Name, dto.Description, dto.CreatedAt);
}
