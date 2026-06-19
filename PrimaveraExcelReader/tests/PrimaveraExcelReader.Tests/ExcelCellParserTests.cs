using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;

namespace PrimaveraExcelReader.Tests;

public sealed class ExcelCellParserTests
{
    [Theory]
    [InlineData("120", 120)]
    [InlineData("48.5", 48.5)]
    [InlineData("1,234.5", 1234.5)]
    public void Parse_Decimal_ParsesInvariantAndCommaDecimalSeparator(string raw, decimal expected)
    {
        decimal? value = ExcelCellParser.Parse<decimal?>(raw, nameof(raw));
        Assert.Equal(expected, value);
    }

    [Fact]
    public void Parse_Decimal_ReturnsNullForEmptyOptionalValue()
    {
        decimal? value = ExcelCellParser.Parse<decimal?>(null, "BudgetedUnits");
        Assert.Null(value);
    }

    [Theory]
    [InlineData("2026-03-01", 2026, 3, 1)]
    [InlineData("03/15/2026", 2026, 3, 15)]
    public void Parse_DateOnly_ParsesCommonPrimaveraFormats(string raw, int year, int month, int day)
    {
        DateOnly? value = ExcelCellParser.Parse<DateOnly?>(raw, "PlannedStart");
        Assert.Equal(new DateOnly(year, month, day), value);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("yes", true)]
    [InlineData("1", true)]
    [InlineData("no", false)]
    [InlineData("0", false)]
    public void Parse_Boolean_ParsesCommonExcelValues(string raw, bool expected)
    {
        bool value = ExcelCellParser.Parse<bool>(raw, "IsActive");
        Assert.Equal(expected, value);
    }

    [Fact]
    public void Parse_ThrowsForInvalidDecimal()
    {
        ExcelCellParseException exception = Assert.Throws<ExcelCellParseException>(
            () => ExcelCellParser.Parse<decimal>("not-a-number", "DurationHours"));

        Assert.Equal("DurationHours", exception.FieldName);
    }
}
