using ImportPipeline.Domain;
using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;

namespace PrimaveraExcelReader.ImportPipeline;

internal static class ImportPipelineRowMapping
{
    public static ExcelRowMapResult<T> TryMap<T>(ExcelSheetProfile<T> profile, ExcelRowData row)
        where T : new()
    {
        IReadOnlyList<ImportConfigRule> rules = ExcelSheetProfileImportRules.FromProfile(profile);
        var mapper = new ImportRowMapper(rules);
        RowMappingResult mappingResult = mapper.Map(ImportRowAdapter.FromExcelRow(row));

        return mappingResult switch
        {
            RowMappingResult.Skipped skipped => ExcelRowMapResult<T>.Failure(
            [
                ExcelReadIssue.MappingError(row.RowIndex + 1, skipped.Reason)
            ]),
            RowMappingResult.Invalid invalid => ExcelRowMapResult<T>.Failure(
                invalid.MissingFields
                    .Select(fieldName => ToRequiredIssue(row, profile, fieldName))
                    .ToList()),
            RowMappingResult.Mapped mapped => Materialize(profile, row, mapped.Values),
            _ => throw new InvalidOperationException($"Unexpected mapping result: {mappingResult.GetType().Name}")
        };
    }

    private static ExcelReadIssue ToRequiredIssue<T>(
        ExcelRowData row,
        ExcelSheetProfile<T> profile,
        string fieldName)
        where T : new()
    {
        ExcelColumnBinding<T> binding = profile.ColumnBindings
            .First(b => string.Equals(b.FieldName, fieldName, StringComparison.Ordinal));

        return ExcelReadIssue.RequiredValueMissing(
            row.RowIndex + 1,
            binding.HeaderName,
            binding.ColumnIndex);
    }

    private static ExcelRowMapResult<T> Materialize<T>(
        ExcelSheetProfile<T> profile,
        ExcelRowData row,
        IReadOnlyList<FieldValue> values)
        where T : new()
    {
        var issues = new List<ExcelReadIssue>();
        var model = new T();
        var valuesByField = values.ToDictionary(v => v.FieldName, StringComparer.Ordinal);

        foreach (ExcelColumnBinding<T> binding in profile.ColumnBindings)
        {
            if (!valuesByField.TryGetValue(binding.FieldName, out FieldValue fieldValue))
            {
                continue;
            }

            try
            {
                binding.Setter(model, fieldValue.Value);
            }
            catch (ExcelCellParseException ex)
            {
                issues.Add(ExcelReadIssue.ParseError(
                    row.RowIndex + 1,
                    ex.FieldName,
                    binding.HeaderName,
                    binding.ColumnIndex,
                    ex.RawValue,
                    ex.TargetType));
            }
        }

        if (issues.Count > 0)
        {
            return ExcelRowMapResult<T>.Failure(issues);
        }

        if (profile.AfterMap is not null)
        {
            try
            {
                model = profile.AfterMap(model, row);
            }
            catch (Exception ex)
            {
                issues.Add(ExcelReadIssue.MappingError(row.RowIndex + 1, ex.Message));
                return ExcelRowMapResult<T>.Failure(issues);
            }
        }

        return ExcelRowMapResult<T>.Success(model);
    }
}
