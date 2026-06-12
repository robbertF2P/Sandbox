using ApiImportActorPoc.Contracts.Models.Planning;
using ApiImportActorPoc.Core.Planning;

namespace ApiImportActorPoc.Api.Endpoints;

public static class PlanningEndpoints
{
    public static IEndpointRouteBuilder MapPlanningEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectId:int}/plan").WithTags("Planning");

        group.MapGet("/", async (
            int projectId,
            PlanningService service,
            CancellationToken cancellationToken) =>
        {
            var plan = await service.GetPlanAsync(projectId, cancellationToken);
            return plan is null ? Results.NotFound() : Results.Ok(plan);
        })
        .WithName("GetProjectPlan")
        .WithSummary("Get calculated Gantt plan for a project");

        group.MapPut("/start", async (
            int projectId,
            SetProjectPlanStartRequest request,
            PlanningService service,
            CancellationToken cancellationToken) =>
        {
            var plan = await service.SetProjectStartAsync(projectId, request.PlannedStartDate, cancellationToken);
            return plan is null ? Results.NotFound() : Results.Ok(plan);
        })
        .WithName("SetProjectPlanStart")
        .WithSummary("Set project planned start and recalculate timeline");

        group.MapPost("/milestones", async (
            int projectId,
            CreateMilestoneRequest request,
            PlanningService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var milestone = await service.AddMilestoneAsync(projectId, request, cancellationToken);
                return milestone is null ? Results.NotFound() : Results.Ok(milestone);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        })
        .WithName("AddProjectMilestone")
        .WithSummary("Add a planning milestone");

        var assignments = app.MapGroup("/api/assignments").WithTags("Planning");

        assignments.MapPut("/{assignmentId:int}/duration", async (
            int assignmentId,
            SetAssignmentDurationRequest request,
            PlanningService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var plan = await service.SetAssignmentDurationAsync(assignmentId, request.DurationDays, cancellationToken);
                return plan is null ? Results.NotFound() : Results.Ok(plan);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        })
        .WithName("SetAssignmentDuration")
        .WithSummary("Set assignment duration in days and recalculate project plan");

        app.MapDelete("/api/milestones/{milestoneId:int}", async (
            int milestoneId,
            PlanningService service,
            CancellationToken cancellationToken) =>
            await service.DeleteMilestoneAsync(milestoneId, cancellationToken)
                ? Results.NoContent()
                : Results.NotFound())
        .WithName("DeleteMilestone")
        .WithSummary("Delete a planning milestone");

        return app;
    }
}
