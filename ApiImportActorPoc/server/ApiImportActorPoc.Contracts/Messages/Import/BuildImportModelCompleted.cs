using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Contracts.Messages.Import;

public sealed record BuildImportModelCompleted(
    Guid SessionId,
    bool Success,
    ProjectModel? Model,
    string? ErrorMessage);
