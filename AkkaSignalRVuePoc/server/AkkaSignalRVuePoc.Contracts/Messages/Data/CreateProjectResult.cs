using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record CreateProjectResult(bool OrganisationExists, ProjectDto? Project);
