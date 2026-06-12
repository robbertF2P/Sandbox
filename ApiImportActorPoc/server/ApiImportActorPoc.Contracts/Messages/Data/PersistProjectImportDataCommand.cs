using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Contracts.Messages.Data;

public sealed record PersistProjectImportDataCommand(Guid SessionId, ProjectModel Model) : IActorSystemMessage;
