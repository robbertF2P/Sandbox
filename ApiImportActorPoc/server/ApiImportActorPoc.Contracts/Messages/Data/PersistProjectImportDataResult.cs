namespace ApiImportActorPoc.Contracts.Messages.Data;

public sealed record PersistProjectImportDataResult(
    bool Success,
    int? ProjectId,
    bool Created,
    string? ErrorMessage);
