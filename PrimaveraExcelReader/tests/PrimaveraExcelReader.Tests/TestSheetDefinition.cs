namespace PrimaveraExcelReader.Tests;

public sealed record TestSheetDefinition(string Name, IReadOnlyList<IReadOnlyList<string>> Rows);
