using F2pPlatform.Host.Contracts.ApprovalQueue;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages;
using F2pPlatform.Host.Core.ApprovalQueue;
using HourApprovals.Application.Ports;

namespace F2pPlatform.Host.ApprovalQueue;

internal static class ApprovalQueueEndpoints
{
    public static WebApplication MapApprovalQueueEndpoints(this WebApplication app)
    {
        app.MapGet("/api/hour-approvals/queue", async (
                HttpContext httpContext,
                string? organisationIds,
                string? submissionCategories,
                string? search,
                IApprovalQueueFacade facade,
                CancellationToken cancellationToken) =>
            {
                if (!await IsFeatureEnabled(httpContext))
                {
                    return Results.NotFound();
                }

                ApprovalQueueFilter filter = ParseFilter(organisationIds, submissionCategories, search);
                GetApprovalQueueReply reply = await facade.QueryAsync(new GetApprovalQueue(filter), cancellationToken);

                return Results.Ok(reply.Rows.Select(MapRow));
            })
            .WithName("GetHourApprovalsQueue")
            .WithTags("HourApprovals")
            .WithSummary(
                "Composed approval queue (Planning + Timekeeping + Hours via actor messages). No duplicate store.");

        return app;
    }

    private static async Task<bool> IsFeatureEnabled(HttpContext httpContext)
    {
        IHourApprovalsFeatureGate featureGate =
            httpContext.RequestServices.GetRequiredService<IHourApprovalsFeatureGate>();

        return featureGate.IsEnabled;
    }

    private static ApprovalQueueFilter ParseFilter(
        string? organisationIds,
        string? submissionCategories,
        string? search)
    {
        IReadOnlyList<int> orgIds = ParseIntList(organisationIds);
        IReadOnlyList<SubmissionCategory> categories = ParseCategories(submissionCategories);
        return new ApprovalQueueFilter(orgIds, categories, search);
    }

    private static IReadOnlyList<int> ParseIntList(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? []
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse)
                .ToList();

    private static IReadOnlyList<SubmissionCategory> ParseCategories(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return [];
        }

        List<SubmissionCategory> categories = [];
        foreach (string part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            SubmissionCategory? category = part.Trim().ToLowerInvariant() switch
            {
                "worked_on" or "worked-on" or "workedon" => SubmissionCategory.WorkedOn,
                "other_active" or "other-active" or "otheractive" => SubmissionCategory.OtherActive,
                "never_submitted" or "never-submitted" or "neversubmitted" => SubmissionCategory.NeverSubmitted,
                _ => null,
            };

            if (category is not null)
            {
                categories.Add(category.Value);
            }
        }

        return categories;
    }

    private static object MapRow(ApprovalQueueRow row) => new
    {
        taskId = row.TaskId,
        assignmentId = row.AssignmentId,
        title = row.Title,
        activityCode = row.ActivityCode,
        organisationLabel = row.OrganisationLabel,
        projectLabel = row.ProjectLabel,
        hoursWorkedInWindow = row.HoursWorkedInWindow,
        submissionCategory = ToApiName(row.SubmissionCategory),
        approvalState = row.ApprovalState,
        isApproved = row.IsApproved,
        currentValues = new
        {
            hoursToGo = row.HoursToGo,
            progress = row.Progress,
            workedHours = row.WorkedHours,
            plannedStart = row.PlannedStart,
            plannedFinish = row.PlannedFinish,
        },
        lastApproval = row.LastSubmittedAtUtc is null
            ? null
            : new
            {
                approvedBy = row.LastSubmittedBy,
                approvedAtUtc = row.LastSubmittedAtUtc,
            },
    };

    private static string ToApiName(SubmissionCategory category) =>
        category switch
        {
            SubmissionCategory.WorkedOn => "worked_on",
            SubmissionCategory.OtherActive => "other_active",
            SubmissionCategory.NeverSubmitted => "never_submitted",
            _ => category.ToString(),
        };
}
