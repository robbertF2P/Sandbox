namespace PrimaveraExcelReader.Mapping;

public enum ExcelReadIssueKind
{
    SheetNotFound,
    HeaderRowMissing,
    EmptyRow,
    FilteredOut,
    RequiredValueMissing,
    ParseError,
    MappingError
}
