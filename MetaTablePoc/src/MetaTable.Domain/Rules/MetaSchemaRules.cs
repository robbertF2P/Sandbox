using MetaTable.Domain.Enums;
using MetaTable.Domain.Models;

namespace MetaTable.Domain.Rules;

public static class MetaSchemaRules
{
    public static void ValidateNewColumn(MetaColumnEntity column, MetaTableEntity owningTable, MetaTableEntity? associationTarget)
    {
        ArgumentNullException.ThrowIfNull(column);
        ArgumentNullException.ThrowIfNull(owningTable);

        if (string.IsNullOrWhiteSpace(column.Name))
        {
            throw new InvalidOperationException("Column name is required.");
        }

        switch (column.Kind)
        {
            case MetaColumnKind.Text:
            case MetaColumnKind.Integer:
                if (column.AssociationTargetTableId is not null)
                {
                    throw new InvalidOperationException(
                        $"{column.Kind} columns must not set AssociationTargetTableId.");
                }

                break;
            case MetaColumnKind.Association:
                if (associationTarget is null)
                {
                    throw new InvalidOperationException(
                        "Association columns require a valid AssociationTargetTableId.");
                }

                if (associationTarget.Id == owningTable.Id)
                {
                    throw new InvalidOperationException("Association target must be a different logical table.");
                }

                break;
            default:
                throw new InvalidOperationException($"Unsupported column kind: {column.Kind}.");
        }
    }

    public static void ValidateRowValues(
        IReadOnlyList<MetaColumnEntity> columns,
        IReadOnlyDictionary<string, string?> textValues,
        IReadOnlyDictionary<string, int?> integerValues,
        IReadOnlyDictionary<string, Guid?> associationValues)
    {
        foreach (MetaColumnEntity column in columns)
        {
            switch (column.Kind)
            {
                case MetaColumnKind.Text:
                    textValues.TryGetValue(column.Name, out string? text);
                    if (column.IsRequired && string.IsNullOrWhiteSpace(text))
                    {
                        throw new InvalidOperationException($"Required text column '{column.Name}' is missing.");
                    }

                    break;
                case MetaColumnKind.Integer:
                    integerValues.TryGetValue(column.Name, out int? integer);
                    if (column.IsRequired && integer is null)
                    {
                        throw new InvalidOperationException($"Required integer column '{column.Name}' is missing.");
                    }

                    break;
                case MetaColumnKind.Association:
                    associationValues.TryGetValue(column.Name, out Guid? target);
                    if (column.IsRequired && target is null)
                    {
                        throw new InvalidOperationException($"Required association column '{column.Name}' is missing.");
                    }

                    break;
            }
        }

        EnsureNoUnknownColumns(columns, textValues.Keys, MetaColumnKind.Text);
        EnsureNoUnknownColumns(columns, integerValues.Keys, MetaColumnKind.Integer);
        EnsureNoUnknownColumns(columns, associationValues.Keys, MetaColumnKind.Association);
    }

    private static void EnsureNoUnknownColumns(
        IReadOnlyList<MetaColumnEntity> columns,
        IEnumerable<string> suppliedNames,
        MetaColumnKind expectedKind)
    {
        HashSet<string> allowed = columns
            .Where(column => column.Kind == expectedKind)
            .Select(column => column.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (string name in suppliedNames)
        {
            if (!allowed.Contains(name))
            {
                throw new InvalidOperationException(
                    $"Column '{name}' is not a declared {expectedKind} column on this table.");
            }
        }
    }
}
