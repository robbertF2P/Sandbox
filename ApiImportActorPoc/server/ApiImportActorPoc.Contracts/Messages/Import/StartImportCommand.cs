using ApiImportActorPoc.Contracts.Models.Import;

namespace ApiImportActorPoc.Contracts.Messages.Import;

public sealed record StartImportCommand(ProjectImportPayload Payload) : IActorSystemMessage;
