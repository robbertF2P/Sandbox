using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Contracts.Messages.Import;

public sealed record RegisterImportSession(Guid SessionId, ProjectModel Model);
