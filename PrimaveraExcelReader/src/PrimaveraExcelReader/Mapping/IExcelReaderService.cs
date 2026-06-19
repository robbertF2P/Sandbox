namespace PrimaveraExcelReader.Mapping;

public interface IExcelReaderService
{
    Task<ExcelReadResult<T>> ReadAsync<T>(
        Stream stream,
        ExcelSheetProfile<T> profile,
        CancellationToken cancellationToken = default)
        where T : new();

    Task<IReadOnlyDictionary<string, ExcelReadResult<T>>> ReadManyAsync<T>(
        Stream stream,
        IReadOnlyList<ExcelSheetProfile<T>> profiles,
        CancellationToken cancellationToken = default)
        where T : new();
}
