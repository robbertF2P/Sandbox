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
            .MapOptional(row => row.Status).From("Status")
            .MapOptional(row => row.PlannedStart).From("Planned Start")
            .MapOptional(row => row.PlannedFinish).From("Planned Finish")
            .MapOptional(row => row.DurationHours).From("Original Duration (h)")
            .Build();

    public static ExcelSheetProfile<PrimaveraActivityRow> LegacyActivityExport { get; } =
        ExcelSheetProfile<PrimaveraActivityRow>.Configure()
            .Sheet("TASK")
            .HeaderRow(0)
            .DataStartsAt(1)
            .Map(row => row.ActivityId).From("task_code", required: true)
            .Map(row => row.ActivityName).From("task_name", required: true)
            .Map(row => row.WbsCode).From("wbs_id", required: true)
            .MapOptional(row => row.Status).From("status_code")
            .MapOptional(row => row.PlannedStart).From("early_start_date")
            .MapOptional(row => row.PlannedFinish).From("early_end_date")
            .MapOptional(row => row.DurationHours).From("target_drtn_hr_cnt")
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
            .MapOptional(row => row.ResourceName).From("Resource Name")
            .MapOptional(row => row.BudgetedUnits).From("Budgeted Units")
            .MapOptional(row => row.RemainingUnits).From("Remaining Units")
            .MapOptional(row => row.TradeCode).From("Trade Code")
            .Build();

    public static ExcelSheetProfile<PrimaveraTaskRow> ResourceAssignmentExport { get; } =
        ExcelSheetProfile<PrimaveraTaskRow>.Configure()
            .Sheet("Resource Assignments")
            .HeaderRow(0)
            .DataStartsAt(1)
            .Map(row => row.TaskId).AtColumn(0, "Assignment ID", required: true)
            .Map(row => row.ActivityId).AtColumn(1, "Activity ID", required: true)
            .MapOptional(row => row.ResourceName).AtColumn(3, "Resource ID")
            .MapOptional(row => row.TradeCode).AtColumn(4, "Role ID")
            .MapOptional(row => row.BudgetedUnits).AtColumn(5, "Budgeted Units")
            .MapOptional(row => row.RemainingUnits).AtColumn(6, "Remaining Units")
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
            .MapOptional(row => row.ParentWbsCode).From("Parent WBS Code")
            .MapOptional(row => row.ProjectId).From("Project ID")
            .Build();
}
