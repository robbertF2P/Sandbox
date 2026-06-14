using Akka.Actor;
using ApiImportActorPoc.Api.Tests.Infrastructure;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Progress;
using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Contracts.Values;
using ApiImportActorPoc.Core.Actors.Progress;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Api.Tests.Actors;

public sealed class ProgressManagerActorTests : ActorTestBase<ProgressManagerActorTests>
{
    public ProgressManagerActorTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public async Task BookHoursCommand_PublishesEventsAndReplies()
    {
        await using var database = new SqlServerTestDatabase();
        await database.InitializeAsync();

        var payload = new ProjectImportPayload(
            "MV Progress Actor",
            [
                new ComponentImportPayload(
                    null,
                    "Block",
                    null,
                    null,
                    [
                        new ActivityImportPayload(
                            null,
                            "Weld",
                            [new AssignmentImportPayload(null, "Sam", null, 32)],
                            null,
                            null)
                    ],
                    null)
            ]);

        var upsertService = new ProjectImportUpsertService(database.Factory);
        var model = ProjectModelBuilder.Build(payload).Model;
        await upsertService.UpsertAsync(model);

        await using var db = await database.Factory.CreateDbContextAsync();
        var assignmentId = await db.Assignments.Select(assignment => assignment.Id).SingleAsync();

        var startedProbe = CreateTestProbe();
        var bookedProbe = CreateTestProbe();
        var recalculatedProbe = CreateTestProbe();
        Sys.EventStream.Subscribe(startedProbe.Ref, typeof(HoursBookedProcessingStarted));
        Sys.EventStream.Subscribe(bookedProbe.Ref, typeof(HoursBooked));
        Sys.EventStream.Subscribe(recalculatedProbe.Ref, typeof(ProgressRecalculated));

        var progressManager = Sys.ActorOf(
            ProgressManagerActor.Props(database.Factory),
            "progress-manager");

        var result = await progressManager.Ask<BookHoursResult>(
            new BookHoursCommand(assignmentId, Hours.From(4m), "Shift A"));

        Assert.True(result.Success);
        Assert.NotNull(result.Booking);
        Assert.Equal(4m, result.Booking.Hours.Value);

        var started = startedProbe.ExpectMsg<HoursBookedProcessingStarted>();
        Assert.Equal(assignmentId, started.AssignmentId);

        var booked = bookedProbe.ExpectMsg<HoursBooked>();
        Assert.Equal(result.Booking.Id, booked.Booking.Id);

        var recalculated = recalculatedProbe.ExpectMsg<ProgressRecalculated>();
        Assert.Equal(4m, recalculated.Progress.HoursWorked.Value);
        Assert.Equal(32m, recalculated.Progress.BudgetedHours.Value);
    }

    [Fact]
    public async Task BookHoursCommand_RejectsNonPositiveHours()
    {
        var progressManager = Sys.ActorOf(
            ProgressManagerActor.Props(new NoOpDbContextFactory()),
            "progress-manager");

        var result = await progressManager.Ask<BookHoursResult>(
            new BookHoursCommand(1, Hours.Zero, null));

        Assert.False(result.Success);
        Assert.Contains("greater than zero", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class NoOpDbContextFactory : IDbContextFactory<ImportDbContext>
    {
        public ImportDbContext CreateDbContext() =>
            throw new NotSupportedException("Database access is not expected for validation-only tests.");

        public Task<ImportDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Database access is not expected for validation-only tests.");
    }
}
