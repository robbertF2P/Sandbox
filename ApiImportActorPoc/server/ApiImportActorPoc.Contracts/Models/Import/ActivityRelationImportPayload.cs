namespace ApiImportActorPoc.Contracts.Models.Import;

public sealed record ActivityRelationImportPayload(
    string RelatedActivityId,
    string Type);
