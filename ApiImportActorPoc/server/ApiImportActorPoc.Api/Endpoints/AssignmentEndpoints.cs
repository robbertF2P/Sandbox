using ApiImportActorPoc.Api.Services;
using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Api.Endpoints;

public static class AssignmentEndpoints
{
    public static IEndpointRouteBuilder MapAssignmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assignments").WithTags("Assignments");

        group.MapGet("/", async (HourBookingService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAssignmentsAsync(cancellationToken)))
            .WithName("ListAssignments")
            .WithSummary("List assignments with project context for hour booking");

        group.MapPost("/{assignmentId:int}/hours", async (
            int assignmentId,
            BookHoursRequest request,
            HourBookingService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var booking = await service.BookHoursAsync(assignmentId, request, cancellationToken);
                return booking is null ? Results.NotFound() : Results.Ok(booking);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        })
        .WithName("BookHours")
        .WithSummary("Book worked hours on an assignment");

        return app;
    }
}
