namespace ApiImportActorPoc.Contracts.Models.Planning;

public sealed record CreateMilestoneRequest(
    string Name,
    DateOnly TargetDate,
    int? ActivityId = null);
