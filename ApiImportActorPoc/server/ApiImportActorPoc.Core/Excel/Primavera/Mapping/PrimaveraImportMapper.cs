using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Excel.Primavera.Models;

namespace ApiImportActorPoc.Core.Excel.Primavera.Mapping;

public static class PrimaveraImportMapper
{
    public static ActivityImportPayload ToActivityImportPayload(PrimaveraActivityRow row)
    {
        return new ActivityImportPayload(
            row.ActivityId,
            row.ActivityName,
            Assignments: null,
            Relations: null,
            ExternalIds: new Dictionary<string, string>
            {
                ["Primavera"] = row.ActivityId,
                ["WBS"] = row.WbsCode
            });
    }

    public static AssignmentImportPayload ToAssignmentImportPayload(PrimaveraTaskRow row)
    {
        decimal? budgetedHours = decimal.TryParse(row.BudgetedUnits, out decimal parsedHours)
            ? parsedHours
            : null;

        return new AssignmentImportPayload(
            row.TaskId,
            row.ResourceName ?? row.TradeCode ?? "Unassigned",
            row.TaskName,
            budgetedHours,
            ExternalIds: new Dictionary<string, string>
            {
                ["Primavera"] = row.TaskId,
                ["Activity"] = row.ActivityId
            });
    }

    public static ComponentImportPayload ToComponentImportPayload(PrimaveraWbsRow row)
    {
        return new ComponentImportPayload(
            row.WbsCode,
            row.WbsName,
            IsTemplate: null,
            ChildComponents: null,
            Activities: null,
            ExternalIds: new Dictionary<string, string>
            {
                ["Primavera"] = row.WbsCode,
                ["Project"] = row.ProjectId ?? string.Empty
            });
    }
}
