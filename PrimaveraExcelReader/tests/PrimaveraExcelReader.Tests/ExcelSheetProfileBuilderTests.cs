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

        ExcelRowData row = ExcelRowFactory.FromCells(
            1,
            ("Code", "A-1"),
            ("Note", "Inspection"));

        ExcelRowMapResult<SampleRow> result = profile.TryMapRow(row);

        SampleRow mapped = ExcelReadResultAssert.Success(result);
        Assert.Equal("A-1", mapped.Code);
        Assert.Equal("Inspection", mapped.Note);
    }

    [Fact]
    public void Bind_SupportsManualSetterForNonStandardMappings()
    {
        ExcelSheetProfile<SampleRow> profile = ExcelSheetProfile<SampleRow>.Configure()
            .Sheet("Sample")
            .Bind("Code", (row, value) => row.Code = value?.ToUpperInvariant() ?? string.Empty, required: true)
            .Build();

        ExcelRowData row = ExcelRowFactory.FromCells(1, ("Code", "a-1"));

        ExcelRowMapResult<SampleRow> result = profile.TryMapRow(row);

        Assert.Equal("A-1", ExcelReadResultAssert.Success(result).Code);
    }

    private sealed class SampleRow
    {
        public string Code { get; set; } = string.Empty;

        public string? Note { get; set; }
    }
}
