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
        IReadOnlyDictionary<AssignmentId, decimal> hoursByAssignmentId,
        IReadOnlyDictionary<TaskId, HourSubmissionSnapshot> snapshotsByTaskId,
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
                snapshot.LastSubmission?.SubmittedAtUtc);

            if (!MatchesSubmissionCategoryFilter(category, snapshot, filter))
            {
                continue;
            }

            if (!MatchesSearchFilter(assignment, filter.Search))
            {
                continue;
            }

            rows.Add(new ApprovalQueueRow(
                assignment.TaskId,
                assignment.AssignmentId,
                assignment.OrganisationId,
                assignment.Labels,
                hoursInWindow,
                category,
                snapshot.ApprovalState,
                snapshot.CurrentValues,
                snapshot.LastSubmission));
        }

        return rows;
    }

    public static SubmissionCategory ResolveCategory(
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

            if (selected == SubmissionCategory.NeverSubmitted && snapshot.LastSubmission is null)
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesSearchFilter(PlanningAssignmentRow assignment, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        string term = search.Trim();
        AssignmentLabels labels = assignment.Labels;
        return labels.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
            || labels.ActivityCode.Contains(term, StringComparison.OrdinalIgnoreCase)
            || labels.ProjectLabel.Contains(term, StringComparison.OrdinalIgnoreCase);
    }
}
