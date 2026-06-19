using PrimaveraExcelReader.Primavera.Models;

namespace PrimaveraExcelReader.Primavera.Mapping;

public static class PrimaveraImportMapper
{
    public static ActivityImportDto ToActivityImportDto(PrimaveraActivityRow row)
    {
        return new ActivityImportDto(
            row.ActivityId,
            row.ActivityName,
            ExternalIds: new Dictionary<string, string>
            {
                ["Primavera"] = row.ActivityId,
                ["WBS"] = row.WbsCode
            });
    }

    public static AssignmentImportDto ToAssignmentImportDto(PrimaveraTaskRow row)
    {
        return new AssignmentImportDto(
            row.TaskId,
            row.ResourceName ?? row.TradeCode ?? "Unassigned",
            row.TaskName,
            row.BudgetedUnits,
            ExternalIds: new Dictionary<string, string>
            {
                ["Primavera"] = row.TaskId,
                ["Activity"] = row.ActivityId
            });
    }

    public static ComponentImportDto ToComponentImportDto(PrimaveraWbsRow row)
    {
        return new ComponentImportDto(
            row.WbsCode,
            row.WbsName,
            row.ParentWbsCode,
            ExternalIds: new Dictionary<string, string>
            {
                ["Primavera"] = row.WbsCode,
                ["Project"] = row.ProjectId ?? string.Empty
            });
    }
}
