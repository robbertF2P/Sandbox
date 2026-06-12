namespace ApiImportActorPoc.Contracts.Messages.Import;

public sealed record PersistImportResult(bool Success, Guid? ProjectId, string? ErrorMessage);
