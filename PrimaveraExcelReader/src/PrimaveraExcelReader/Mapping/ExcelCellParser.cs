using System.Globalization;

namespace PrimaveraExcelReader.Mapping;

public static class ExcelCellParser
{
    private static readonly string[] DateOnlyFormats =
    [
        "yyyy-MM-dd",
        "yyyy/MM/dd",
        "MM/dd/yyyy",
        "dd/MM/yyyy",
        "dd-MMM-yyyy",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd HH:mm:ss"
    ];

    public static T Parse<T>(string? raw, string fieldName)
    {
        Type declaredType = typeof(T);
        Type underlyingType = Nullable.GetUnderlyingType(declaredType) ?? declaredType;
        bool isNullable = declaredType != underlyingType;

        if (string.IsNullOrWhiteSpace(raw))
        {
            if (underlyingType == typeof(string))
            {
                return declaredType == typeof(string)
                    ? (T)(object)string.Empty
                    : default!;
            }

            if (isNullable)
            {
                return default!;
            }

            if (underlyingType.IsValueType)
            {
                return default!;
            }
        }

        object parsed = ParseNonEmpty(raw!.Trim(), fieldName, underlyingType);
        return (T)parsed;
    }

    private static object ParseNonEmpty(string raw, string fieldName, Type targetType)
    {
        if (targetType == typeof(string))
        {
            return raw;
        }

        if (targetType == typeof(int))
        {
            return ParseInt32(raw, fieldName);
        }

        if (targetType == typeof(long))
        {
            return ParseInt64(raw, fieldName);
        }

        if (targetType == typeof(decimal))
        {
            return ParseDecimal(raw, fieldName);
        }

        if (targetType == typeof(double))
        {
            return ParseDouble(raw, fieldName);
        }

        if (targetType == typeof(float))
        {
            return ParseSingle(raw, fieldName);
        }

        if (targetType == typeof(bool))
        {
            return ParseBoolean(raw, fieldName);
        }

        if (targetType == typeof(DateTime))
        {
            return ParseDateTime(raw, fieldName);
        }

        if (targetType == typeof(DateOnly))
        {
            return ParseDateOnly(raw, fieldName);
        }

        if (targetType == typeof(TimeSpan))
        {
            return ParseTimeSpan(raw, fieldName);
        }

        if (targetType == typeof(Guid))
        {
            return ParseGuid(raw, fieldName);
        }

        if (targetType.IsEnum)
        {
            return ParseEnum(raw, fieldName, targetType);
        }

        throw new NotSupportedException(
            $"Type '{targetType.Name}' is not supported by Map(). Use Bind() for custom conversions.");
    }

    private static int ParseInt32(string raw, string fieldName)
    {
        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
        {
            return value;
        }

        throw new ExcelCellParseException(fieldName, raw, typeof(int));
    }

    private static long ParseInt64(string raw, string fieldName)
    {
        if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
        {
            return value;
        }

        throw new ExcelCellParseException(fieldName, raw, typeof(long));
    }

    private static decimal ParseDecimal(string raw, string fieldName)
    {
        if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
        {
            return value;
        }

        string normalized = raw.Replace(',', '.');
        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            return value;
        }

        throw new ExcelCellParseException(fieldName, raw, typeof(decimal));
    }

    private static double ParseDouble(string raw, string fieldName)
    {
        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            return value;
        }

        throw new ExcelCellParseException(fieldName, raw, typeof(double));
    }

    private static float ParseSingle(string raw, string fieldName)
    {
        if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
        {
            return value;
        }

        throw new ExcelCellParseException(fieldName, raw, typeof(float));
    }

    private static bool ParseBoolean(string raw, string fieldName)
    {
        if (bool.TryParse(raw, out bool value))
        {
            return value;
        }

        if (string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(raw, "0", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "n", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "no", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        throw new ExcelCellParseException(fieldName, raw, typeof(bool));
    }

    private static DateTime ParseDateTime(string raw, string fieldName)
    {
        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime value))
        {
            return value;
        }

        if (DateTime.TryParseExact(
                raw,
                DateOnlyFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out value))
        {
            return value;
        }

        throw new ExcelCellParseException(fieldName, raw, typeof(DateTime));
    }

    private static DateOnly ParseDateOnly(string raw, string fieldName)
    {
        if (DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateOnly value))
        {
            return value;
        }

        if (DateOnly.TryParseExact(
                raw,
                DateOnlyFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out value))
        {
            return value;
        }

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime dateTime))
        {
            return DateOnly.FromDateTime(dateTime);
        }

        throw new ExcelCellParseException(fieldName, raw, typeof(DateOnly));
    }

    private static TimeSpan ParseTimeSpan(string raw, string fieldName)
    {
        if (TimeSpan.TryParse(raw, CultureInfo.InvariantCulture, out TimeSpan value))
        {
            return value;
        }

        throw new ExcelCellParseException(fieldName, raw, typeof(TimeSpan));
    }

    private static Guid ParseGuid(string raw, string fieldName)
    {
        if (Guid.TryParse(raw, out Guid value))
        {
            return value;
        }

        throw new ExcelCellParseException(fieldName, raw, typeof(Guid));
    }

    private static object ParseEnum(string raw, string fieldName, Type enumType)
    {
        if (Enum.TryParse(enumType, raw, ignoreCase: true, out object? value) && value is not null)
        {
            return value;
        }

        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int numericValue)
            && Enum.IsDefined(enumType, numericValue))
        {
            return Enum.ToObject(enumType, numericValue);
        }

        throw new ExcelCellParseException(fieldName, raw, enumType);
    }
}
