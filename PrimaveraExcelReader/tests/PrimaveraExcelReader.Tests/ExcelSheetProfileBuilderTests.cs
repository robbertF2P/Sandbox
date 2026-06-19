using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;

namespace PrimaveraExcelReader.Tests;

public sealed class ExcelSheetProfileBuilderTests
{
    [Fact]
    public void Build_ThrowsWhenSheetNameMissing()
    {
        ExcelSheetProfileBuilder<SampleRow> builder = ExcelSheetProfile<SampleRow>.Configure()
            .Map(row => row.Code).From("Code");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Contains("Sheet name is required", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Map_CompilesPropertySetterFromExpression()
    {
        ExcelSheetProfile<SampleRow> profile = ExcelSheetProfile<SampleRow>.Configure()
            .Sheet("Sample")
            .Map(row => row.Code).From("Code", required: true)
            .MapOptional(row => row.Note).From("Note")
            .Build();

        var row = new ExcelRowData(
            1,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["Code"] = "A-1",
                ["Note"] = "Inspection"
            },
            ["A-1", "Inspection"]);

        ExcelRowMapResult<SampleRow> result = profile.TryMapRow(row);

        Assert.True(result.IsSuccess);
        Assert.Equal("A-1", result.Row!.Code);
        Assert.Equal("Inspection", result.Row.Note);
    }

    [Fact]
    public void Bind_SupportsManualSetterForNonStandardMappings()
    {
        ExcelSheetProfile<SampleRow> profile = ExcelSheetProfile<SampleRow>.Configure()
            .Sheet("Sample")
            .Bind("Code", (row, value) => row.Code = value?.ToUpperInvariant() ?? string.Empty, required: true)
            .Build();

        var row = new ExcelRowData(
            1,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { ["Code"] = "a-1" },
            ["a-1"]);

        ExcelRowMapResult<SampleRow> result = profile.TryMapRow(row);

        Assert.True(result.IsSuccess);
        Assert.Equal("A-1", result.Row!.Code);
    }

    private sealed class SampleRow
    {
        public string Code { get; set; } = string.Empty;

        public string? Note { get; set; }
    }
}
