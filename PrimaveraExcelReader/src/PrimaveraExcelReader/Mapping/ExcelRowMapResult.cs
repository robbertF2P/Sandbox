namespace PrimaveraExcelReader.Mapping;

public sealed record ExcelRowMapResult<T>(T? Row, IReadOnlyList<ExcelReadIssue> Issues)
{
    public bool IsSuccess => Issues.Count == 0 && Row is not null;

    public static ExcelRowMapResult<T> Success(T row) => new(row, []);

    public static ExcelRowMapResult<T> Failure(IReadOnlyList<ExcelReadIssue> issues) => new(default, issues);
}
