using MetaTable.Domain.Enums;
using MetaTable.Domain.Models;
using MetaTable.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace MetaTable.Infrastructure;

public sealed class MetaTableStore(MetaTableDbContext dbContext)
{
    public async Task<MetaTableEntity> DefineTableAsync(string key, string displayName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Table key is required.");
        }

        string normalizedKey = key.Trim();
        bool exists = await dbContext.MetaTables.AnyAsync(table => table.Key == normalizedKey, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Table key '{normalizedKey}' already exists.");
        }

        MetaTableEntity table = new()
        {
            Id = Guid.NewGuid(),
            Key = normalizedKey,
            DisplayName = displayName.Trim(),
        };

        dbContext.MetaTables.Add(table);
        await dbContext.SaveChangesAsync(cancellationToken);
        return table;
    }

    public async Task<MetaColumnEntity> DefineColumnAsync(
        string tableKey,
        string columnName,
        MetaColumnKind kind,
        bool isRequired = false,
        int sortOrder = 0,
        string? associationTargetTableKey = null,
        CancellationToken cancellationToken = default)
    {
        MetaTableEntity table = await RequireTableByKeyAsync(tableKey, cancellationToken);

        MetaTableEntity? associationTarget = null;
        Guid? associationTargetId = null;
        if (kind == MetaColumnKind.Association)
        {
            if (string.IsNullOrWhiteSpace(associationTargetTableKey))
            {
                throw new InvalidOperationException("Association columns require associationTargetTableKey.");
            }

            associationTarget = await RequireTableByKeyAsync(associationTargetTableKey, cancellationToken);
            associationTargetId = associationTarget.Id;
        }

        MetaColumnEntity column = new()
        {
            Id = Guid.NewGuid(),
            TableId = table.Id,
            Name = columnName.Trim(),
            Kind = kind,
            AssociationTargetTableId = associationTargetId,
            IsRequired = isRequired,
            SortOrder = sortOrder,
        };

        MetaSchemaRules.ValidateNewColumn(column, table, associationTarget);

        dbContext.MetaColumns.Add(column);
        await dbContext.SaveChangesAsync(cancellationToken);
        return column;
    }

    public async Task<MetaRowEntity> InsertRowAsync(
        string tableKey,
        IReadOnlyDictionary<string, string?> textValues,
        IReadOnlyDictionary<string, int?> integerValues,
        IReadOnlyDictionary<string, Guid?> associationValues,
        CancellationToken cancellationToken = default)
    {
        MetaTableEntity table = await RequireTableByKeyAsync(tableKey, cancellationToken);
        List<MetaColumnEntity> columns = await dbContext.MetaColumns
            .Where(column => column.TableId == table.Id)
            .ToListAsync(cancellationToken);

        MetaSchemaRules.ValidateRowValues(columns, textValues, integerValues, associationValues);

        MetaRowEntity row = new()
        {
            Id = Guid.NewGuid(),
            TableId = table.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        dbContext.MetaRows.Add(row);

        foreach (MetaColumnEntity column in columns)
        {
            switch (column.Kind)
            {
                case MetaColumnKind.Text:
                    if (textValues.TryGetValue(column.Name, out string? text) && text is not null)
                    {
                        dbContext.MetaTextCells.Add(new MetaTextCellEntity
                        {
                            RowId = row.Id,
                            ColumnId = column.Id,
                            Value = text,
                        });
                    }

                    break;
                case MetaColumnKind.Integer:
                    if (integerValues.TryGetValue(column.Name, out int? integer) && integer is not null)
                    {
                        dbContext.MetaIntegerCells.Add(new MetaIntegerCellEntity
                        {
                            RowId = row.Id,
                            ColumnId = column.Id,
                            Value = integer.Value,
                        });
                    }

                    break;
                case MetaColumnKind.Association:
                    if (associationValues.TryGetValue(column.Name, out Guid? targetRowId) && targetRowId is not null)
                    {
                        await EnsureAssociationTargetAsync(column, targetRowId.Value, cancellationToken);
                        dbContext.MetaAssociationCells.Add(new MetaAssociationCellEntity
                        {
                            RowId = row.Id,
                            ColumnId = column.Id,
                            TargetRowId = targetRowId.Value,
                        });
                    }

                    break;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return row;
    }

    public async Task<MetaRowSnapshot?> GetRowSnapshotAsync(Guid rowId, CancellationToken cancellationToken = default)
    {
        MetaRowEntity? row = await dbContext.MetaRows
            .AsNoTracking()
            .Include(metaRow => metaRow.Table)
            .FirstOrDefaultAsync(metaRow => metaRow.Id == rowId, cancellationToken);

        if (row is null)
        {
            return null;
        }

        List<MetaColumnEntity> columns = await dbContext.MetaColumns
            .AsNoTracking()
            .Where(column => column.TableId == row.TableId)
            .OrderBy(column => column.SortOrder)
            .ToListAsync(cancellationToken);

        Dictionary<string, string?> textValues = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, int?> integerValues = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, Guid?> associationValues = new(StringComparer.OrdinalIgnoreCase);

        foreach (MetaColumnEntity column in columns)
        {
            switch (column.Kind)
            {
                case MetaColumnKind.Text:
                    MetaTextCellEntity? textCell = await dbContext.MetaTextCells
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cell => cell.RowId == rowId && cell.ColumnId == column.Id, cancellationToken);
                    textValues[column.Name] = textCell?.Value;
                    break;
                case MetaColumnKind.Integer:
                    MetaIntegerCellEntity? integerCell = await dbContext.MetaIntegerCells
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cell => cell.RowId == rowId && cell.ColumnId == column.Id, cancellationToken);
                    integerValues[column.Name] = integerCell?.Value;
                    break;
                case MetaColumnKind.Association:
                    MetaAssociationCellEntity? associationCell = await dbContext.MetaAssociationCells
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cell => cell.RowId == rowId && cell.ColumnId == column.Id, cancellationToken);
                    associationValues[column.Name] = associationCell?.TargetRowId;
                    break;
            }
        }

        return new MetaRowSnapshot
        {
            RowId = row.Id,
            TableKey = row.Table.Key,
            TextValues = textValues,
            IntegerValues = integerValues,
            AssociationValues = associationValues,
        };
    }

    public async Task<MetaTableDefinition?> GetTableDefinitionAsync(string tableKey, CancellationToken cancellationToken = default)
    {
        MetaTableEntity? table = await dbContext.MetaTables
            .AsNoTracking()
            .FirstOrDefaultAsync(metaTable => metaTable.Key == tableKey, cancellationToken);

        if (table is null)
        {
            return null;
        }

        List<MetaColumnDefinition> columns = await dbContext.MetaColumns
            .AsNoTracking()
            .Where(column => column.TableId == table.Id)
            .OrderBy(column => column.SortOrder)
            .Select(column => new MetaColumnDefinition
            {
                Id = column.Id,
                TableId = column.TableId,
                Name = column.Name,
                Kind = column.Kind,
                AssociationTargetTableId = column.AssociationTargetTableId,
                IsRequired = column.IsRequired,
                SortOrder = column.SortOrder,
            })
            .ToListAsync(cancellationToken);

        return new MetaTableDefinition
        {
            Id = table.Id,
            Key = table.Key,
            DisplayName = table.DisplayName,
            Columns = columns,
        };
    }

    private async Task EnsureAssociationTargetAsync(
        MetaColumnEntity column,
        Guid targetRowId,
        CancellationToken cancellationToken)
    {
        if (column.AssociationTargetTableId is null)
        {
            throw new InvalidOperationException($"Association column '{column.Name}' has no target table.");
        }

        MetaRowEntity? targetRow = await dbContext.MetaRows
            .AsNoTracking()
            .FirstOrDefaultAsync(row => row.Id == targetRowId, cancellationToken);

        if (targetRow is null)
        {
            throw new InvalidOperationException($"Association target row '{targetRowId}' does not exist.");
        }

        if (targetRow.TableId != column.AssociationTargetTableId)
        {
            throw new InvalidOperationException(
                $"Association target row '{targetRowId}' belongs to a different logical table than declared for column '{column.Name}'.");
        }
    }

    private async Task<MetaTableEntity> RequireTableByKeyAsync(string tableKey, CancellationToken cancellationToken)
    {
        MetaTableEntity? table = await dbContext.MetaTables
            .FirstOrDefaultAsync(metaTable => metaTable.Key == tableKey, cancellationToken);

        return table ?? throw new InvalidOperationException($"Unknown table key '{tableKey}'.");
    }
}
