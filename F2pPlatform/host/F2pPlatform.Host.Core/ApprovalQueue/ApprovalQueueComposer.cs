using F2pPlatform.Host.Contracts.ApprovalQueue;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Hours;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Planning;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Timekeeping;

namespace F2pPlatform.Host.Core.ApprovalQueue;

/// <summary>
/// Pure merge logic — no database, no HTTP. Unit-tested.
/// </summary>
public static class ApprovalQueueComposer
{
    public static IReadOnlyList<ApprovalQueueRow> Compose(
        IReadOnlyList<PlanningAssignmentRow> assignments,
        IReadOnlyDictionary<Guid, decimal> hoursByAssignmentId,
        IReadOnlyDictionary<Guid, HourSubmissionSnapshot> snapshotsByTaskId,
        ApprovalQueueFilter filter)
    {
        List<ApprovalQueueRow> rows = [];

        foreach (PlanningAssignmentRow assignment in assignments)
        {
            if (!MatchesOrganisationFilter(assignment, filter))
            {
                continue;
            }

            hoursByAssignmentId.TryGetValue(assignment.AssignmentId, out decimal hoursInWindow);
            if (!snapshotsByTaskId.TryGetValue(assignment.TaskId, out HourSubmissionSnapshot? snapshot))
            {
                continue;
            }

            SubmissionCategory category = ResolveCategory(
                hoursInWindow,
                assignment.IsActiveAssignment,
                snapshot.LastSubmittedAtUtc);

            if (!MatchesSubmissionCategoryFilter(category, snapshot, filter))
            {
                continue;
            }

            if (!MatchesSearchFilter(assignment, snapshot, filter.Search))
            {
                continue;
            }

            rows.Add(new ApprovalQueueRow(
                assignment.TaskId,
                assignment.AssignmentId,
                assignment.Title,
                assignment.ActivityCode,
                assignment.OrganisationLabel,
                assignment.ProjectLabel,
                hoursInWindow,
                category,
                snapshot.ApprovalState,
                snapshot.IsApproved,
                snapshot.HoursToGo,
                snapshot.Progress,
                snapshot.WorkedHours,
                snapshot.PlannedStart,
                snapshot.PlannedFinish,
                snapshot.LastSubmittedBy,
                snapshot.LastSubmittedAtUtc));
        }

        return rows;
    }

    internal static SubmissionCategory ResolveCategory(
        decimal hoursInWindow,
        bool isActiveAssignment,
        DateTimeOffset? lastSubmittedAtUtc)
    {
        if (hoursInWindow > 0)
        {
            return SubmissionCategory.WorkedOn;
        }

        if (isActiveAssignment)
        {
            return SubmissionCategory.OtherActive;
        }

        return lastSubmittedAtUtc is null
            ? SubmissionCategory.NeverSubmitted
            : SubmissionCategory.OtherActive;
    }

    private static bool MatchesOrganisationFilter(PlanningAssignmentRow assignment, ApprovalQueueFilter filter) =>
        filter.OrganisationIds.Count == 0 || filter.OrganisationIds.Contains(assignment.OrganisationId);

    private static bool MatchesSubmissionCategoryFilter(
        SubmissionCategory category,
        HourSubmissionSnapshot snapshot,
        ApprovalQueueFilter filter)
    {
        if (filter.SubmissionCategories.Count == 0)
        {
            return true;
        }

        foreach (SubmissionCategory selected in filter.SubmissionCategories)
        {
            if (selected == category)
            {
                return true;
            }

            if (selected == SubmissionCategory.NeverSubmitted && snapshot.LastSubmittedAtUtc is null)
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesSearchFilter(
        PlanningAssignmentRow assignment,
        HourSubmissionSnapshot snapshot,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        string term = search.Trim();
        return assignment.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
            || assignment.ActivityCode.Contains(term, StringComparison.OrdinalIgnoreCase)
            || assignment.ProjectLabel.Contains(term, StringComparison.OrdinalIgnoreCase);
    }
}
