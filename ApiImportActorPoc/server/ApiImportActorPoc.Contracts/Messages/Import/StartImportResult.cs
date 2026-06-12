namespace ApiImportActorPoc.Contracts.Messages.Import;

public sealed record StartImportResult(Guid SessionId, bool Accepted, string? ErrorMessage);
