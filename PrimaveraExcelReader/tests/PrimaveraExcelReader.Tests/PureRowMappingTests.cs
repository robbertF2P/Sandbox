using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Primavera.Models;
using PrimaveraExcelReader.Primavera.Profiles;

namespace PrimaveraExcelReader.Tests;

public sealed class PureRowMappingTests
{
    public static TheoryData<ExcelRowData, Action<PrimaveraActivityRow>> StandardActivityRowCases =>
        new()
        {
            {
                ExcelRowFactory.FromHeaderRowAndValues(
                    1,
                    PrimaveraSheetScenarios.StandardActivityHeaders,
                    ["A-100", "Hull Block Erection", "WBS-204", "In Progress", "2026-03-01", "2026-03-15", "120"]),
                row =>
                {
                    Assert.Equal("A-100", row.ActivityId);
                    Assert.Equal("Hull Block Erection", row.ActivityName);
                    Assert.Equal(120m, row.DurationHours);
                    Assert.Equal(new DateOnly(2026, 3, 1), row.PlannedStart);
                }
            },
            {
                ExcelRowFactory.FromCells(
                    1,
                    ("Activity ID", "A-300"),
                    ("Activity Name", "Propulsion Alignment"),
                    ("WBS Code", "WBS-301"),
                    ("Status", "Complete"),
                    ("Planned Start", "2026-02-01"),
                    ("Planned Finish", "2026-02-10"),
                    ("Original Duration (h)", "40")),
                row => Assert.Equal("A-300", row.ActivityId)
            }
        };

    [Theory]
    [MemberData(nameof(StandardActivityRowCases))]
    public void TryMapRow_MapsStandardPrimaveraActivityRows(ExcelRowData row, Action<PrimaveraActivityRow> assert)
    {
        ExcelRowMapResult<PrimaveraActivityRow> result =
            PrimaveraSheetProfiles.StandardActivityExport.TryMapRow(row);

        assert(ExcelReadResultAssert.Success(result));
    }

    [Fact]
    public void MapRows_CollectsAllColumnIssuesOnSameRow()
    {
        ExcelSheetProfile<TypedSampleRow> profile = ExcelSheetProfile<TypedSampleRow>.Configure()
            .Sheet("Rows")
            .Map(row => row.Code).From("Code", required: true)
            .Map(row => row.Hours).From("Hours")
            .Map(row => row.StartDate).From("Start")
            .Build();

        ExcelRowData row = ExcelRowFactory.FromCells(
            1,
            ("Code", "A-1"),
            ("Hours", "bad-hours"),
            ("Start", "bad-date"));

        (IReadOnlyList<TypedSampleRow> rows, IReadOnlyList<ExcelReadIssue> issues) =
            RowMappingPipeline.MapRows(profile, [row]);

        Assert.Empty(rows);
        Assert.Equal(2, issues.Count);
        Assert.All(issues, issue => Assert.Equal(2, issue.RowNumber));
    }

    [Fact]
    public void MapRows_FiltersLegacyMilestoneRows()
    {
        ExcelRowData taskRow = ExcelRowFactory.FromCells(
            1,
            ("task_code", "A-500"),
            ("task_name", "Steel Cutting"),
            ("wbs_id", "WBS-500"),
            ("task_type", "TT_Task"),
            ("status_code", "Active"),
            ("early_start_date", "2026-01-01"),
            ("early_end_date", "2026-01-05"),
            ("target_drtn_hr_cnt", "32"));

        ExcelRowData milestoneRow = ExcelRowFactory.FromCells(
            2,
            ("task_code", "M-001"),
            ("task_name", "Keel Laying Milestone"),
            ("wbs_id", "WBS-500"),
            ("task_type", "TT_Mile"),
            ("status_code", "Complete"),
            ("early_start_date", "2026-01-06"),
            ("early_end_date", "2026-01-06"),
            ("target_drtn_hr_cnt", "0"));

        (IReadOnlyList<PrimaveraActivityRow> rows, IReadOnlyList<ExcelReadIssue> issues) =
            RowMappingPipeline.MapRows(
                PrimaveraSheetProfiles.LegacyActivityExport,
                [taskRow, milestoneRow]);

        PrimaveraActivityRow activity = Assert.Single(rows);
        Assert.Equal("A-500", activity.ActivityId);
        ExcelReadResultAssert.HasIssue(issues, ExcelReadIssueKind.FilteredOut, 3);
    }

    private sealed class TypedSampleRow
    {
        public string Code { get; set; } = string.Empty;

        public decimal? Hours { get; set; }

        public DateOnly? StartDate { get; set; }
    }
}
