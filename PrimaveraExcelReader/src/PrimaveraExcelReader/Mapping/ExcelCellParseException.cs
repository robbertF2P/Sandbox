namespace PrimaveraExcelReader.Mapping;

public sealed class ExcelCellParseException : Exception
{
    public ExcelCellParseException(string fieldName, string? rawValue, Type targetType)
        : base(BuildMessage(fieldName, rawValue, targetType))
    {
        FieldName = fieldName;
        RawValue = rawValue;
        TargetType = targetType;
    }

    public string FieldName { get; }

    public string? RawValue { get; }

    public Type TargetType { get; }

    private static string BuildMessage(string fieldName, string? rawValue, Type targetType)
    {
        string displayValue = rawValue is null ? "<null>" : $"'{rawValue}'";
        return $"Could not parse {displayValue} into {targetType.Name} for '{fieldName}'.";
    }
}
