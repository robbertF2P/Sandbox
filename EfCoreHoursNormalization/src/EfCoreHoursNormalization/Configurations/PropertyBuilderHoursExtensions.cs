using EfCoreHoursNormalization.Conversions;
using EfCoreHoursNormalization.Values;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfCoreHoursNormalization.Configurations;

public static class PropertyBuilderHoursExtensions
{
    /// <summary>
    /// Maps <see cref="Hours"/> to a SQL Server <c>real</c> column with read/write normalization.
    /// Works with SQLite <c>REAL</c> in unit tests via the same converter.
    /// </summary>
    public static PropertyBuilder<Hours> HasHoursColumn(
        this PropertyBuilder<Hours> propertyBuilder,
        string? columnName = null)
    {
        var builder = propertyBuilder
            .HasConversion(HoursValueConverter.Instance, HoursValueComparer.Instance)
            .HasColumnType("real");

        if (columnName is not null)
        {
            builder.HasColumnName(columnName);
        }

        return builder;
    }
}
