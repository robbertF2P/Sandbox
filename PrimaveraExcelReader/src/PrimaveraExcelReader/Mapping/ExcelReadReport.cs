namespace PrimaveraExcelReader.Mapping;

public static class ExcelReadReport
{
    public static string FormatIssues(IReadOnlyList<ExcelReadIssue> issues)
    {
        if (issues.Count == 0)
        {
            return "No issues.";
        }

        IEnumerable<string> lines = issues.Select(FormatIssue);
        return string.Join(Environment.NewLine, lines);
    }

    public static string FormatIssue(ExcelReadIssue issue)
    {
        if (issue.RowNumber <= 0)
        {
            return $"[Sheet] {issue.Message}";
        }

        string location = issue.ColumnHeader ?? issue.FieldName ?? "row";
        return $"Row {issue.RowNumber}, {location}: {issue.Message}";
    }
}
