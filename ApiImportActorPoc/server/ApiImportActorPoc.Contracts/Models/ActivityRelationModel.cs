using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Models;

public sealed record ActivityRelationModel(
    int RelatedActivityId,
    ActivityRelationType Type,
    LagDays LagDays = default);
