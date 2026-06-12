using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Core.Planning;

public sealed record PlanningAssignmentSnapshot(
    int Id,
    string Label,
    decimal DurationDays);

public sealed record PlanningActivitySnapshot(
    int Id,
    string Name,
    string ComponentName,
    IReadOnlyList<PlanningAssignmentSnapshot> Assignments,
    IReadOnlyList<ActivityRelationModel> Relations);
