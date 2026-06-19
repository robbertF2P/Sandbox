namespace PrimaveraExcelReader.Primavera.Models;

public sealed record ActivityImportDto(
    string? Id,
    string Name,
    IReadOnlyDictionary<string, string>? ExternalIds = null);

public sealed record AssignmentImportDto(
    string? Id,
    string PersonName,
    string? Description,
    decimal? BudgetedHours = null,
    IReadOnlyDictionary<string, string>? ExternalIds = null);

public sealed record ComponentImportDto(
    string? Id,
    string Name,
    string? ParentId,
    IReadOnlyDictionary<string, string>? ExternalIds = null);
