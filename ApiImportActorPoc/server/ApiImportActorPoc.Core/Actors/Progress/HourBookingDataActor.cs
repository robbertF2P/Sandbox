using Akka.Actor;
using Akka.Event;
using ApiImportActorPoc.Contracts.Messages.Progress;
using ApiImportActorPoc.Core.Progress;
using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;
using Platform.Serilog.Logging.Correlation;

namespace ApiImportActorPoc.Core.Actors.Progress;

public sealed class HourBookingDataActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly HourBookingPersistService _persistService;

    public HourBookingDataActor(IDbContextFactory<ImportDbContext> dbContextFactory)
    {
        _persistService = new HourBookingPersistService(dbContextFactory);
        ReceiveAsync<CorrelatedMessageEnvelope>(DispatchAsync);
    }

    public static Props Props(IDbContextFactory<ImportDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new HourBookingDataActor(dbContextFactory));

    private async Task DispatchAsync(CorrelatedMessageEnvelope envelope)
    {
        if (envelope.Message is not PersistHourBookingCommand command)
        {
            Unhandled(envelope);
            return;
        }

        var sender = Sender;

        try
        {
            var outcome = await _persistService.PersistAsync(
                command.AssignmentId,
                command.Hours,
                command.Notes);

            if (!outcome.Success)
            {
                sender.Tell(new PersistHourBookingResult(false, null, null, outcome.ErrorMessage));
                _log.Warning(
                    "Hour booking persist failed for processing {0}: {1}",
                    command.ProcessingId,
                    outcome.ErrorMessage);
                return;
            }

            sender.Tell(new PersistHourBookingResult(
                true,
                outcome.Booking,
                outcome.ProjectId,
                null));

            _log.Info(
                "Persisted hour booking {0} for assignment {1} (processing {2})",
                outcome.Booking!.Id,
                command.AssignmentId,
                command.ProcessingId);
        }
        catch (Exception exception)
        {
            _log.Error(exception, "Failed to persist hour booking for processing {0}", command.ProcessingId);
            sender.Tell(new PersistHourBookingResult(false, null, null, exception.Message));
        }
    }
}
