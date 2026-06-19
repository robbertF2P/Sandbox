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

    [Fact]
    public void MapRow_SkipsRowWhenTypedColumnCannotBeParsed()
    {
        ExcelSheetProfile<TypedRow> profile = ExcelSheetProfile<TypedRow>.Configure()
            .Sheet("Sample")
            .Map(row => row.Code).From("Code", required: true)
            .Map(row => row.Hours).From("Hours")
            .Build();

        var row = new ExcelRowData(
            1,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["Code"] = "A-1",
                ["Hours"] = "invalid"
            },
            ["A-1", "invalid"]);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => profile.MapRow(row));
        Assert.Contains("Row 2", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MapRow_MapsTypedColumnsFromProfile()
    {
        ExcelSheetProfile<TypedRow> profile = ExcelSheetProfile<TypedRow>.Configure()
            .Sheet("Sample")
            .Map(row => row.Code).From("Code", required: true)
            .Map(row => row.Hours).From("Hours")
            .Map(row => row.StartDate).From("Start")
            .Build();

        var row = new ExcelRowData(
            1,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["Code"] = "A-1",
                ["Hours"] = "96",
                ["Start"] = "2026-04-01"
            },
            ["A-1", "96", "2026-04-01"]);

        TypedRow mapped = profile.MapRow(row);

        Assert.Equal("A-1", mapped.Code);
        Assert.Equal(96m, mapped.Hours);
        Assert.Equal(new DateOnly(2026, 4, 1), mapped.StartDate);
    }

    private sealed class TypedRow
    {
        public string Code { get; set; } = string.Empty;

        public decimal? Hours { get; set; }

        public DateOnly? StartDate { get; set; }
    }
}
