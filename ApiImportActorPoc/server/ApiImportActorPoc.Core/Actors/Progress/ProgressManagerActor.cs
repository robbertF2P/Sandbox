using Akka.Actor;
using Akka.Event;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Progress;
using ApiImportActorPoc.Contracts.Values;
using ApiImportActorPoc.Core.Progress;
using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Actors.Progress;

public sealed class ProgressManagerActor : ReceiveActor
{
    private readonly IDbContextFactory<ImportDbContext> _dbContextFactory;
    private readonly ProjectProgressLoader _progressLoader;
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private IActorRef _hourBookingData = ActorRefs.Nobody;

    public ProgressManagerActor(IDbContextFactory<ImportDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _progressLoader = new ProjectProgressLoader(dbContextFactory);
        Become(Ready);
    }

    public static Props Props(IDbContextFactory<ImportDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new ProgressManagerActor(dbContextFactory));

    protected override void PreStart()
    {
        _hourBookingData = Context.ActorOf(
            HourBookingDataActor.Props(_dbContextFactory),
            "hour-booking-data");
    }

    private void Ready()
    {
        Receive<BookHoursCommand>(HandleBookHours);
    }

    private void HandleBookHours(BookHoursCommand command)
    {
        if (command.Hours <= Hours.Zero)
        {
            Sender.Tell(new BookHoursResult(false, null, "Hours must be greater than zero."));
            return;
        }

        var processingId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        Context.System.EventStream.Publish(new HoursBookedProcessingStarted(
            processingId,
            command.AssignmentId,
            command.Hours,
            occurredAt));

        _log.Info(
            "Progress manager started hour booking processing {0} for assignment {1}",
            processingId,
            command.AssignmentId);

        _hourBookingData.Tell(new PersistHourBookingCommand(
            processingId,
            command.AssignmentId,
            command.Hours,
            command.Notes));

        Become(() => WaitForPersist(processingId, command.AssignmentId, Sender));
    }

    private void WaitForPersist(Guid processingId, int assignmentId, IActorRef originalSender)
    {
        ReceiveAsync<PersistHourBookingResult>(async result =>
        {
            if (!result.Success || result.Booking is null || result.ProjectId is not int projectId)
            {
                var errorMessage = result.ErrorMessage ?? "Hour booking failed.";
                Context.System.EventStream.Publish(new HoursBookingFailed(
                    processingId,
                    assignmentId,
                    errorMessage,
                    DateTimeOffset.UtcNow));

                originalSender.Tell(new BookHoursResult(false, null, errorMessage));
                _log.Warning(
                    "Progress manager failed processing {0} for assignment {1}: {2}",
                    processingId,
                    assignmentId,
                    errorMessage);
                Become(Ready);
                return;
            }

            var occurredAt = DateTimeOffset.UtcNow;
            Context.System.EventStream.Publish(new HoursBooked(
                processingId,
                result.Booking,
                projectId,
                occurredAt));

            var projectProgress = await _progressLoader.LoadAsync(projectId);
            if (projectProgress is not null)
            {
                Context.System.EventStream.Publish(new ProgressRecalculated(
                    processingId,
                    projectId,
                    projectProgress.Progress,
                    DateTimeOffset.UtcNow));
            }

            originalSender.Tell(new BookHoursResult(true, result.Booking, null));
            _log.Info(
                "Progress manager completed processing {0} for assignment {1} on project {2}",
                processingId,
                assignmentId,
                projectId);

            Become(Ready);
        });
    }
}
