namespace ApiImportActorPoc.Contracts.Models;

public sealed record ActivityModel(
    Guid Id,
    string Name,
    IReadOnlyList<AssignmentModel> Assignments,
    IReadOnlyList<ActivityRelationModel> Relations,
    IReadOnlyDictionary<string, string> ExternalIds);
