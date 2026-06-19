namespace PrimaveraExcelReader.Tests;

public static class PrimaveraSheetScenarios
{
    public static readonly string[] StandardActivityHeaders =
    [
        "Activity ID",
        "Activity Name",
        "WBS Code",
        "Status",
        "Planned Start",
        "Planned Finish",
        "Original Duration (h)"
    ];

    public static TestSheetDefinition StandardActivitiesSheet { get; } = new(
        "Activities",
        [
            StandardActivityHeaders,
            ["A-100", "Hull Block Erection", "WBS-204", "In Progress", "2026-03-01", "2026-03-15", "120"],
            ["A-200", "Engine Room Outfitting", "WBS-205", "Not Started", "2026-03-16", "2026-04-01", "80"]
        ]);

    public static TestSheetDefinition MixedQualityActivitiesSheet { get; } = new(
        "Activities",
        [
            StandardActivityHeaders,
            ["A-100", "Valid Activity", "WBS-100", "Active", "2026-03-01", "2026-03-15", "120"],
            ["A-200", "Bad Duration", "WBS-200", "Active", "2026-03-01", "2026-03-15", "not-a-number"],
            ["", "Missing Id", "WBS-300", "Active", "2026-03-01", "2026-03-15", "80"]
        ]);

    public static TestSheetDefinition LegacyTaskSheet { get; } = new(
        "TASK",
        [
            ["task_code", "task_name", "wbs_id", "task_type", "status_code", "early_start_date", "early_end_date", "target_drtn_hr_cnt"],
            ["A-500", "Steel Cutting", "WBS-500", "TT_Task", "Active", "2026-01-01", "2026-01-05", "32"],
            ["M-001", "Keel Laying Milestone", "WBS-500", "TT_Mile", "Complete", "2026-01-06", "2026-01-06", "0"]
        ]);

    public static TestSheetDefinition ResourceAssignmentSheet { get; } = new(
        "Resource Assignments",
        [
            ["Assignment ID", "Activity ID", "Resource Description", "Resource ID", "Role ID", "Budgeted Units", "Remaining Units"],
            ["T-100", "A-100", "Welding crew lead", "RES-WELD-01", "WELD", "160", "120"]
        ]);
}
