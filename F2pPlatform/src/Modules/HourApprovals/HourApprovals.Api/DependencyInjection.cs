using HourApprovals.Application;
using HourApprovals.Application.Ports;
using HourApprovals.Domain.Enums;
using HourApprovals.Domain.Rules;
using HourApprovals.Domain.ValueObjects;
using HourApprovals.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Shared.Domain;

namespace HourApprovals.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddHourApprovalsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHourApprovalsApplication();
        services.AddHourApprovalsInfrastructure(configuration);
        return services;
    }

    public static WebApplication MapHourApprovalsModule(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.MapHourApprovalsEndpoints();
        return app;
    }
}

internal static class HourApprovalsEndpoints
{
    public static IEndpointRouteBuilder MapHourApprovalsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/hour-approvals")
            .WithTags("HourApprovals");

        group.MapGet("/capabilities", async (
                HttpContext httpContext,
                IHourApprovalsService service,
                CancellationToken cancellationToken) =>
            {
                if (!await TryEnsureFeatureEnabled(httpContext))
                {
                    return Results.NotFound();
                }

                HourApprovalsCapabilities capabilities = await service.GetCapabilitiesAsync(
                    ResolvePermissions(httpContext),
                    cancellationToken);

                return Results.Ok(capabilities);
            })
            .WithName("GetHourApprovalsCapabilities")
            .WithSummary("Tenant feature flag, customization pack display settings, and user permissions.");

        group.MapGet("/tasks", async (
                HttpContext httpContext,
                string? approvalStatus,
                IHourApprovalsService service,
                CancellationToken cancellationToken) =>
            {
                if (!await TryEnsureFeatureEnabled(httpContext))
                {
                    return Results.NotFound();
                }

                ApprovalFilterStatus filter = ParseFilter(approvalStatus);
                IReadOnlyList<TaskApprovalView> tasks = await service.ListTasksAsync(filter, cancellationToken);
                return Results.Ok(tasks.Select(MapTask));
            })
            .WithName("ListHourApprovalTasks")
            .WithSummary("List active tasks with approval state and last approval metadata.");

        group.MapGet("/tasks/{taskId:guid}", async (
                HttpContext httpContext,
                Guid taskId,
                IHourApprovalsService service,
                CancellationToken cancellationToken) =>
            {
                if (!await TryEnsureFeatureEnabled(httpContext))
                {
                    return Results.NotFound();
                }

                TaskApprovalView? task = await service.GetTaskAsync(new TaskId(taskId), cancellationToken);
                return task is null ? Results.NotFound() : Results.Ok(MapTask(task));
            })
            .WithName("GetHourApprovalTask")
            .WithSummary("Get one task with approval metadata.");

        group.MapPut("/tasks/{taskId:guid}", async (
                HttpContext httpContext,
                Guid taskId,
                SaveTaskBody body,
                IHourApprovalsService service,
                CancellationToken cancellationToken) =>
            {
                if (!await TryEnsureFeatureEnabled(httpContext))
                {
                    return Results.NotFound();
                }

                try
                {
                    TaskApprovalView saved = await service.SaveTaskAsync(
                        new TaskId(taskId),
                        MapValues(body),
                        ResolveUserName(httpContext),
                        ResolvePermissions(httpContext),
                        cancellationToken);

                    return Results.Ok(MapTask(saved));
                }
                catch (UnauthorizedAccessException exception)
                {
                    return Results.Json(new { error = exception.Message }, statusCode: StatusCodes.Status403Forbidden);
                }
                catch (KeyNotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("SaveHourApprovalTask")
            .WithSummary("Save task values and auto-approve as the acting user.");

        group.MapPost("/tasks/{taskId:guid}/approve", async (
                HttpContext httpContext,
                Guid taskId,
                IHourApprovalsService service,
                CancellationToken cancellationToken) =>
            {
                if (!await TryEnsureFeatureEnabled(httpContext))
                {
                    return Results.NotFound();
                }

                try
                {
                    TaskApprovalView approved = await service.ApproveTaskAsync(
                        new TaskId(taskId),
                        ResolveUserName(httpContext),
                        ResolvePermissions(httpContext),
                        cancellationToken);

                    return Results.Ok(MapTask(approved));
                }
                catch (UnauthorizedAccessException exception)
                {
                    return Results.Json(new { error = exception.Message }, statusCode: StatusCodes.Status403Forbidden);
                }
                catch (KeyNotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("ApproveHourApprovalTask")
            .WithSummary("Explicitly approve current task values.");

        group.MapPost("/submit", async (
                HttpContext httpContext,
                SubmitTasksBody body,
                IHourApprovalsService service,
                CancellationToken cancellationToken) =>
            {
                if (!await TryEnsureFeatureEnabled(httpContext))
                {
                    return Results.NotFound();
                }

                try
                {
                    SubmitTasksResult result = await service.SubmitTasksAsync(
                        body.TaskIds.Select(id => new TaskId(id)).ToList(),
                        ResolveUserName(httpContext),
                        ResolvePermissions(httpContext),
                        cancellationToken);

                    return Results.Ok(new
                    {
                        approved = result.Approved.Select(MapTask),
                        failures = result.Failures.Select(failure => new
                        {
                            taskId = failure.TaskId.Value,
                            error = failure.Error,
                        }),
                    });
                }
                catch (UnauthorizedAccessException exception)
                {
                    return Results.Json(new { error = exception.Message }, statusCode: StatusCodes.Status403Forbidden);
                }
            })
            .WithName("SubmitHourApprovalTasks")
            .WithSummary("Batch-approve selected tasks (submission).");

        return app;
    }

    private static async Task<bool> TryEnsureFeatureEnabled(HttpContext httpContext)
    {
        IHourApprovalsFeatureGate featureGate =
            httpContext.RequestServices.GetRequiredService<IHourApprovalsFeatureGate>();

        return featureGate.IsEnabled;
    }

    private static UserName ResolveUserName(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-User-Name", out var header)
            && !string.IsNullOrWhiteSpace(header))
        {
            return new UserName(header.ToString());
        }

        return new UserName("anonymous");
    }

    private static IReadOnlyList<string> ResolvePermissions(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue("X-User-Permissions", out var header)
            || string.IsNullOrWhiteSpace(header))
        {
            return [];
        }

        return header.ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static ApprovalFilterStatus ParseFilter(string? approvalStatus) =>
        approvalStatus?.Trim().ToLowerInvariant() switch
        {
            "approved" => ApprovalFilterStatus.Approved,
            "not_approved" or "not-approved" or "notapproved" => ApprovalFilterStatus.NotApproved,
            _ => ApprovalFilterStatus.All,
        };

    private static ApprovalValues MapValues(SaveTaskBody body) =>
        new(
            body.HoursToGo,
            body.Progress,
            body.WorkedHours,
            body.PlannedStart,
            body.PlannedFinish);

    private static object MapTask(TaskApprovalView view) => new
    {
        id = view.Task.Id.Value,
        title = view.Task.Title.Value,
        activityCode = view.Task.ActivityCode.Value,
        isActiveForCurrentUser = view.Task.IsActiveForCurrentUser,
        approvalState = view.State.ToString(),
        isApproved = view.State == TaskApprovalState.Approved,
        currentValues = MapValues(view.Task.CurrentValues),
        lastApproval = view.LastApproval is null
            ? null
            : new
            {
                id = view.LastApproval.Id,
                approvedBy = view.LastApproval.ApprovedBy.Value,
                approvedAtUtc = view.LastApproval.ApprovedAtUtc,
                approvedValues = MapValues(view.LastApproval.ApprovedValues),
            },
    };

    private static object MapValues(ApprovalValues values) => new
    {
        hoursToGo = values.HoursToGo,
        progress = values.Progress,
        workedHours = values.WorkedHours,
        plannedStart = values.PlannedStart,
        plannedFinish = values.PlannedFinish,
    };

    private sealed record SaveTaskBody(
        decimal HoursToGo,
        decimal Progress,
        decimal WorkedHours,
        DateOnly? PlannedStart,
        DateOnly? PlannedFinish);

    private sealed record SubmitTasksBody(IReadOnlyList<Guid> TaskIds);
}
