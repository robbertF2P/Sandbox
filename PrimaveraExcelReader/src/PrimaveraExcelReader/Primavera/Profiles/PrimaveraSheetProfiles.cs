using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Primavera.Models;

namespace PrimaveraExcelReader.Primavera.Profiles;

public static class PrimaveraSheetProfiles
{
    public static ExcelSheetProfile<PrimaveraActivityRow> StandardActivityExport { get; } =
        ExcelSheetProfile<PrimaveraActivityRow>.Configure()
            .Sheet("Activities")
            .HeaderRow(0)
            .DataStartsAt(1)
            .Map(row => row.ActivityId).From("Activity ID", required: true)
            .Map(row => row.ActivityName).From("Activity Name", required: true)
            .Map(row => row.WbsCode).From("WBS Code", required: true)
            .Map(row => row.Status).From("Status")
            .Map(row => row.PlannedStart).From("Planned Start")
            .Map(row => row.PlannedFinish).From("Planned Finish")
            .Map(row => row.DurationHours).From("Original Duration (h)")
            .Build();

    public static ExcelSheetProfile<PrimaveraActivityRow> LegacyActivityExport { get; } =
        ExcelSheetProfile<PrimaveraActivityRow>.Configure()
            .Sheet("TASK")
            .HeaderRow(0)
            .DataStartsAt(1)
            .Map(row => row.ActivityId).From("task_code", required: true)
            .Map(row => row.ActivityName).From("task_name", required: true)
            .Map(row => row.WbsCode).From("wbs_id", required: true)
            .Map(row => row.Status).From("status_code")
            .Map(row => row.PlannedStart).From("early_start_date")
            .Map(row => row.PlannedFinish).From("early_end_date")
            .Map(row => row.DurationHours).From("target_drtn_hr_cnt")
            .Where(row => !string.Equals(row.GetByHeader("task_type"), "TT_Mile", StringComparison.OrdinalIgnoreCase))
            .Build();

    public static ExcelSheetProfile<PrimaveraTaskRow> StandardTaskExport { get; } =
        ExcelSheetProfile<PrimaveraTaskRow>.Configure()
            .Sheet("Tasks")
            .HeaderRow(0)
            .DataStartsAt(1)
            .Map(row => row.TaskId).From("Task ID", required: true)
            .Map(row => row.ActivityId).From("Activity ID", required: true)
            .Map(row => row.TaskName).From("Task Name", required: true)
            .Map(row => row.ResourceName).From("Resource Name")
            .Map(row => row.BudgetedUnits).From("Budgeted Units")
            .Map(row => row.RemainingUnits).From("Remaining Units")
            .Map(row => row.TradeCode).From("Trade Code")
            .Build();

    public static ExcelSheetProfile<PrimaveraTaskRow> ResourceAssignmentExport { get; } =
        ExcelSheetProfile<PrimaveraTaskRow>.Configure()
            .Sheet("Resource Assignments")
            .HeaderRow(0)
            .DataStartsAt(1)
            .Map(row => row.TaskId).AtColumn(0, "Assignment ID", required: true)
            .Map(row => row.ActivityId).AtColumn(1, "Activity ID", required: true)
            .Map(row => row.ResourceName).AtColumn(3, "Resource ID")
            .Map(row => row.TradeCode).AtColumn(4, "Role ID")
            .Map(row => row.BudgetedUnits).AtColumn(5, "Budgeted Units")
            .Map(row => row.RemainingUnits).AtColumn(6, "Remaining Units")
            .AfterMap((model, row) =>
            {
                model.TaskName = row.GetByIndex(2) ?? model.TaskId;
                return model;
            })
            .Build();

    public static ExcelSheetProfile<PrimaveraWbsRow> StandardWbsExport { get; } =
        ExcelSheetProfile<PrimaveraWbsRow>.Configure()
            .Sheet("WBS")
            .HeaderRow(0)
            .DataStartsAt(1)
            .Map(row => row.WbsCode).From("WBS Code", required: true)
            .Map(row => row.WbsName).From("WBS Name", required: true)
            .Map(row => row.ParentWbsCode).From("Parent WBS Code")
            .Map(row => row.ProjectId).From("Project ID")
            .Build();
}
