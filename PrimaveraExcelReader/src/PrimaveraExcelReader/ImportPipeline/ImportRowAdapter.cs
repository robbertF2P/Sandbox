using ImportPipeline.Domain;
using PrimaveraExcelReader.Abstractions;

namespace PrimaveraExcelReader.ImportPipeline;

public static class ImportRowAdapter
{
    public static ImportRow FromExcelRow(ExcelRowData row) => new(row.AsHeaderCells());
}
