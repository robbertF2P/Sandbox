using F2pPlatform.Host.Contracts.ApprovalQueue;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages;
using F2pPlatform.Host.Core.ApprovalQueue;
using HourApprovals.Application.Ports;
using Platform.Shared.Domain;

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
                IHourApprovalsCustomizationPack customizationPack,
                CancellationToken cancellationToken) =>
            {
                if (!await IsFeatureEnabled(httpContext))
                {
                    return Results.NotFound();
                }

                ApprovalQueueFilter filter = ParseFilter(organisationIds, submissionCategories, search);
                GetApprovalQueueReply reply = await facade.QueryAsync(new GetApprovalQueue(filter), cancellationToken);

                IReadOnlyList<Guid> taskIds = reply.Rows.Select(row => row.TaskId.Value).ToList();
                IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, object?>> extensionsByTaskId =
                    customizationPack.GetRowExtensions(taskIds);

                return Results.Ok(reply.Rows.Select(row => MapRow(
                    row,
                    extensionsByTaskId.GetValueOrDefault(row.TaskId.Value) ?? new Dictionary<string, object?>())));
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
        IReadOnlyList<OrganisationId> orgIds = ParseOrganisationIds(organisationIds);
        IReadOnlyList<SubmissionCategory> categories = ParseCategories(submissionCategories);
        return new ApprovalQueueFilter(orgIds, categories, search);
    }

    private static IReadOnlyList<OrganisationId> ParseOrganisationIds(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? []
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => new OrganisationId(int.Parse(part)))
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

    private static object MapRow(
        ApprovalQueueRow row,
        IReadOnlyDictionary<string, object?> extensions) => new
    {
        taskId = row.TaskId.Value,
        assignmentId = row.AssignmentId.Value,
        organisationId = row.OrganisationId.Value,
        title = row.Labels.Title.Value,
        activityCode = row.Labels.ActivityCode.Value,
        organisationLabel = row.Labels.OrganisationLabel.Value,
        projectLabel = row.Labels.ProjectLabel.Value,
        taskNumber = row.Labels.TaskNumber.Value,
        locationPath = row.Labels.LocationPath,
        disciplineLabel = row.Labels.DisciplineLabel,
        teamCount = row.Labels.TeamCount,
        totalHoursBooked = row.Labels.TotalHoursBooked,
        hoursWorkedInWindow = row.HoursWorkedInWindow,
        submissionCategory = ToApiName(row.SubmissionCategory),
        approvalState = row.ApprovalState.ToString(),
        isApproved = row.ApprovalState == ApprovalState.Approved,
        currentValues = MapValues(row.CurrentValues),
        lookbackValues = MapValues(row.LookbackBaseline),
        extensions,
        computed = MapComputed(row),
        lastApproval = row.LastSubmission is null
            ? null
            : new
            {
                approvedBy = row.LastSubmission.SubmittedBy.Value,
                approvedAtUtc = row.LastSubmission.SubmittedAtUtc,
                approvedValues = row.LastSubmission.ApprovedValues is null
                    ? null
                    : MapValues(row.LastSubmission.ApprovedValues),
            },
    };

    private static object MapComputed(ApprovalQueueRow row) => new
    {
        daysSinceLastSubmission = ComputeDaysSinceLastSubmission(row.LastSubmission?.SubmittedAtUtc),
    };

    private static int? ComputeDaysSinceLastSubmission(DateTimeOffset? submittedAtUtc)
    {
        if (submittedAtUtc is null)
        {
            return null;
        }

        return Math.Max(0, (int)(DateTimeOffset.UtcNow - submittedAtUtc.Value).TotalDays);
    }

    private static object MapValues(ApprovalProgressValues values) => new
    {
        hoursToGo = values.HoursToGo,
        progress = values.Progress,
        workedHours = values.WorkedHours,
        plannedStart = values.PlannedStart?.ToString("yyyy-MM-dd"),
        plannedFinish = values.PlannedFinish?.ToString("yyyy-MM-dd"),
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
