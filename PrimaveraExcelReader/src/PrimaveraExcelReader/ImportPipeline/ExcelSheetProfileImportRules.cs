using ImportPipeline.Domain;
using PrimaveraExcelReader.Mapping;

namespace PrimaveraExcelReader.ImportPipeline;

public static class ExcelSheetProfileImportRules
{
    public static IReadOnlyList<ImportConfigRule> FromProfile<T>(ExcelSheetProfile<T> profile)
        where T : new()
    {
        return profile.ColumnBindings
            .Select((binding, index) => new ImportConfigRule(
                Id: index + 1,
                From: binding.HeaderName,
                To: binding.FieldName,
                DefaultValue: null,
                IsRequired: binding.Required,
                SkipIfEmpty: false,
                UseShortValue: false,
                RuleType: ImportConfigRuleType.Attribute))
            .ToList();
    }
}
