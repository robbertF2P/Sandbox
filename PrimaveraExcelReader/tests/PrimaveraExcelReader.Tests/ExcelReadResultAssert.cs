using PrimaveraExcelReader.Mapping;

namespace PrimaveraExcelReader.Tests;

public static class ExcelReadResultAssert
{
    public static T Success<T>(ExcelRowMapResult<T> result)
    {
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Row);
        return result.Row;
    }

    public static void Failure<T>(ExcelRowMapResult<T> result, int expectedIssueCount)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedIssueCount, result.Issues.Count);
    }

    public static void HasIssue(
        IReadOnlyList<ExcelReadIssue> issues,
        ExcelReadIssueKind kind,
        int rowNumber)
    {
        Assert.Contains(issues, issue => issue.Kind == kind && issue.RowNumber == rowNumber);
    }

    public static void HasRows<T>(ExcelReadResult<T> result, int expectedCount)
    {
        Assert.Equal(expectedCount, result.Rows.Count);
    }

    public static void HasNoIssues<T>(ExcelReadResult<T> result)
    {
        Assert.Empty(result.Issues);
    }
}
