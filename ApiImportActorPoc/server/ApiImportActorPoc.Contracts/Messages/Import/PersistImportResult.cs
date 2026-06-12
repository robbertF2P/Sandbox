namespace ApiImportActorPoc.Contracts.Messages.Import;

public sealed record PersistImportResult(bool Success, int? ProjectId, string? ErrorMessage);
