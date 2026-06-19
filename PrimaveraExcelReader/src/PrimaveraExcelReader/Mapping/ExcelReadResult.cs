namespace PrimaveraExcelReader.Mapping;

public sealed record ExcelReadResult<T>(
    string SheetName,
    IReadOnlyList<T> Rows,
    IReadOnlyList<ExcelReadIssue> Issues)
{
    public bool HasIssues => Issues.Count > 0;

    public IReadOnlyList<ExcelReadIssue> RowIssues =>
        Issues.Where(issue => issue.Kind is not ExcelReadIssueKind.SheetNotFound and not ExcelReadIssueKind.HeaderRowMissing).ToArray();

    public IReadOnlyList<ExcelReadIssue> IssuesForRow(int rowNumber) =>
        Issues.Where(issue => issue.RowNumber == rowNumber).ToArray();
}
