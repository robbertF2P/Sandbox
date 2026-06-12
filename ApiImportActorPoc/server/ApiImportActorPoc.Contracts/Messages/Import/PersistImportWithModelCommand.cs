using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Contracts.Messages.Import;

public sealed record PersistImportWithModelCommand(Guid SessionId, ProjectModel Model) : IActorSystemMessage;
