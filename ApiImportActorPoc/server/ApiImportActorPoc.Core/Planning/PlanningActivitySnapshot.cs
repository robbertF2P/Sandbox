using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Core.Planning;

public sealed record PlanningAssignmentSnapshot(
    int Id,
    string Label,
    DurationDays DurationDays);

public sealed record PlanningActivitySnapshot(
    int Id,
    string Name,
    string ComponentName,
    IReadOnlyList<PlanningAssignmentSnapshot> Assignments,
    IReadOnlyList<ActivityRelationModel> Relations);
