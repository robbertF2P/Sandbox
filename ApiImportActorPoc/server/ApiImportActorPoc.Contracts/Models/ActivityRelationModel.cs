namespace ApiImportActorPoc.Contracts.Models;

public sealed record ActivityRelationModel(
    Guid RelatedActivityId,
    ActivityRelationType Type);
