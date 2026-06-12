using Akka.Actor;
using Akka.Event;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Import;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Actors.Import;

public sealed class PersistActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly ProjectImportUpsertService _upsertService;

    public PersistActor(IDbContextFactory<ImportDbContext> dbContextFactory)
    {
        _upsertService = new ProjectImportUpsertService(dbContextFactory);
        ReceiveAsync<PersistImportWithModelCommand>(HandlePersistAsync);
    }

    public static Props Props(IDbContextFactory<ImportDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new PersistActor(dbContextFactory));

    private async Task HandlePersistAsync(PersistImportWithModelCommand command)
    {
        try
        {
            var result = await _upsertService.UpsertAsync(command.Model);

            Context.System.EventStream.Publish(new ImportPersisted(command.SessionId, result.ProjectId, DateTimeOffset.UtcNow));
            Sender.Tell(new PersistImportResult(true, result.ProjectId, null));
            _log.Info(
                "Persisted import session {0} as project {1} ({2})",
                command.SessionId,
                result.ProjectId,
                result.Created ? "created" : "updated");
        }
        catch (Exception exception)
        {
            _log.Error(exception, "Failed to persist import session {0}", command.SessionId);
            Sender.Tell(new PersistImportResult(false, null, exception.Message));
        }
    }
}
