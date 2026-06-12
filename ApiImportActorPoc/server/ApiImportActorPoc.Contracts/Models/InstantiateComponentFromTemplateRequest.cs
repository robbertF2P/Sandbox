namespace ApiImportActorPoc.Contracts.Models;

public sealed record InstantiateComponentFromTemplateRequest(
    string Name,
    int? ParentComponentId = null);
