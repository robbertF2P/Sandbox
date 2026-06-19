using ApiImportActorPoc.Core.Excel.Mapping;
using ApiImportActorPoc.Core.Excel.Primavera.Models;

namespace ApiImportActorPoc.Core.Excel.Primavera.Profiles;

public static class PrimaveraSheetProfiles
{
    public static ExcelSheetProfile<PrimaveraActivityRow> StandardActivityExport { get; } = new()
    {
        SheetName = "Activities",
        HeaderRowIndex = 0,
        DataStartRowIndex = 1,
        ColumnBindings =
        [
            new ExcelColumnBinding<PrimaveraActivityRow>("Activity ID", (row, value) => row.ActivityId = value ?? string.Empty, required: true),
            new ExcelColumnBinding<PrimaveraActivityRow>("Activity Name", (row, value) => row.ActivityName = value ?? string.Empty, required: true),
            new ExcelColumnBinding<PrimaveraActivityRow>("WBS Code", (row, value) => row.WbsCode = value ?? string.Empty, required: true),
            new ExcelColumnBinding<PrimaveraActivityRow>("Status", (row, value) => row.Status = value),
            new ExcelColumnBinding<PrimaveraActivityRow>("Planned Start", (row, value) => row.PlannedStart = value),
            new ExcelColumnBinding<PrimaveraActivityRow>("Planned Finish", (row, value) => row.PlannedFinish = value),
            new ExcelColumnBinding<PrimaveraActivityRow>("Original Duration (h)", (row, value) => row.DurationHours = value)
        ]
    };

    public static ExcelSheetProfile<PrimaveraActivityRow> LegacyActivityExport { get; } = new()
    {
        SheetName = "TASK",
        HeaderRowIndex = 0,
        DataStartRowIndex = 1,
        ColumnBindings =
        [
            new ExcelColumnBinding<PrimaveraActivityRow>("task_code", (row, value) => row.ActivityId = value ?? string.Empty, required: true),
            new ExcelColumnBinding<PrimaveraActivityRow>("task_name", (row, value) => row.ActivityName = value ?? string.Empty, required: true),
            new ExcelColumnBinding<PrimaveraActivityRow>("wbs_id", (row, value) => row.WbsCode = value ?? string.Empty, required: true),
            new ExcelColumnBinding<PrimaveraActivityRow>("status_code", (row, value) => row.Status = value),
            new ExcelColumnBinding<PrimaveraActivityRow>("early_start_date", (row, value) => row.PlannedStart = value),
            new ExcelColumnBinding<PrimaveraActivityRow>("early_end_date", (row, value) => row.PlannedFinish = value),
            new ExcelColumnBinding<PrimaveraActivityRow>("target_drtn_hr_cnt", (row, value) => row.DurationHours = value)
        ],
        RowFilter = row => !string.Equals(row.GetByHeader("task_type"), "TT_Mile", StringComparison.OrdinalIgnoreCase)
    };

    public static ExcelSheetProfile<PrimaveraTaskRow> StandardTaskExport { get; } = new()
    {
        SheetName = "Tasks",
        HeaderRowIndex = 0,
        DataStartRowIndex = 1,
        ColumnBindings =
        [
            new ExcelColumnBinding<PrimaveraTaskRow>("Task ID", (row, value) => row.TaskId = value ?? string.Empty, required: true),
            new ExcelColumnBinding<PrimaveraTaskRow>("Activity ID", (row, value) => row.ActivityId = value ?? string.Empty, required: true),
            new ExcelColumnBinding<PrimaveraTaskRow>("Task Name", (row, value) => row.TaskName = value ?? string.Empty, required: true),
            new ExcelColumnBinding<PrimaveraTaskRow>("Resource Name", (row, value) => row.ResourceName = value),
            new ExcelColumnBinding<PrimaveraTaskRow>("Budgeted Units", (row, value) => row.BudgetedUnits = value),
            new ExcelColumnBinding<PrimaveraTaskRow>("Remaining Units", (row, value) => row.RemainingUnits = value),
            new ExcelColumnBinding<PrimaveraTaskRow>("Trade Code", (row, value) => row.TradeCode = value)
        ]
    };

    public static ExcelSheetProfile<PrimaveraTaskRow> ResourceAssignmentExport { get; } = new()
    {
        SheetName = "Resource Assignments",
        HeaderRowIndex = 0,
        DataStartRowIndex = 1,
        ColumnBindings =
        [
            new ExcelColumnBinding<PrimaveraTaskRow>("Assignment ID", (row, value) => row.TaskId = value ?? string.Empty, required: true, columnIndex: 0),
            new ExcelColumnBinding<PrimaveraTaskRow>("Activity ID", (row, value) => row.ActivityId = value ?? string.Empty, required: true, columnIndex: 1),
            new ExcelColumnBinding<PrimaveraTaskRow>("Resource ID", (row, value) => row.ResourceName = value, columnIndex: 3),
            new ExcelColumnBinding<PrimaveraTaskRow>("Budgeted Units", (row, value) => row.BudgetedUnits = value, columnIndex: 5),
            new ExcelColumnBinding<PrimaveraTaskRow>("Remaining Units", (row, value) => row.RemainingUnits = value, columnIndex: 6),
            new ExcelColumnBinding<PrimaveraTaskRow>("Role ID", (row, value) => row.TradeCode = value, columnIndex: 4)
        ],
        AfterMap = (model, row) =>
        {
            model.TaskName = row.GetByIndex(2) ?? model.TaskId;
            return model;
        }
    };

    public static ExcelSheetProfile<PrimaveraWbsRow> StandardWbsExport { get; } = new()
    {
        SheetName = "WBS",
        HeaderRowIndex = 0,
        DataStartRowIndex = 1,
        ColumnBindings =
        [
            new ExcelColumnBinding<PrimaveraWbsRow>("WBS Code", (row, value) => row.WbsCode = value ?? string.Empty, required: true),
            new ExcelColumnBinding<PrimaveraWbsRow>("WBS Name", (row, value) => row.WbsName = value ?? string.Empty, required: true),
            new ExcelColumnBinding<PrimaveraWbsRow>("Parent WBS Code", (row, value) => row.ParentWbsCode = value),
            new ExcelColumnBinding<PrimaveraWbsRow>("Project ID", (row, value) => row.ProjectId = value)
        ]
    };
}
