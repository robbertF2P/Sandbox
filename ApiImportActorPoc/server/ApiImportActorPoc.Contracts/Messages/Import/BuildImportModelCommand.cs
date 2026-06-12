using ApiImportActorPoc.Contracts.Models.Import;

namespace ApiImportActorPoc.Contracts.Messages.Import;

public sealed record BuildImportModelCommand(Guid SessionId, ProjectImportPayload Payload);
