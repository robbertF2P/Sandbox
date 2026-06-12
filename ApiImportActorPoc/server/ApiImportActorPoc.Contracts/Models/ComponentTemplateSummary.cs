namespace ApiImportActorPoc.Contracts.Models;

public sealed record ComponentTemplateSummary(
    int Id,
    string Name,
    int ActivityCount,
    int AssignmentCount);
