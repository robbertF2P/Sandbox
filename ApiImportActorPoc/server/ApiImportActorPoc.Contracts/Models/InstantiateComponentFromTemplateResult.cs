namespace ApiImportActorPoc.Contracts.Models;

public sealed record InstantiateComponentFromTemplateResult(
    int ComponentId,
    int ActivityCount,
    int AssignmentCount);
