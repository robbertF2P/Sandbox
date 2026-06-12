namespace ApiImportActorPoc.Contracts.Models;

public sealed record ActivityRelationModel(
    int RelatedActivityId,
    ActivityRelationType Type,
    int LagDays = 0);
