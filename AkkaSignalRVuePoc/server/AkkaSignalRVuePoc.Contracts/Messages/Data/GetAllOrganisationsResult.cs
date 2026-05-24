using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record GetAllOrganisationsResult(IReadOnlyList<OrganisationDto> Organisations);
