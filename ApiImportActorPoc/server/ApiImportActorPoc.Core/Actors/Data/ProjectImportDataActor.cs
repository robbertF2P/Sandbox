using Akka.Actor;
using Akka.Event;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Messages.Data;
using ApiImportActorPoc.Core.Import;
using ApiImportActorPoc.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Actors.Data;

public sealed class ProjectImportDataActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly ProjectImportUpsertService _upsertService;

    public ProjectImportDataActor(IDbContextFactory<ImportDbContext> dbContextFactory)
    {
        _upsertService = new ProjectImportUpsertService(dbContextFactory);
        ReceiveAsync<PersistProjectImportDataCommand>(HandlePersistAsync);
    }

    public static Props Props(IDbContextFactory<ImportDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new ProjectImportDataActor(dbContextFactory));

    private async Task HandlePersistAsync(PersistProjectImportDataCommand command)
    {
        try
        {
            var result = await _upsertService.UpsertAsync(
                command.Model,
                progress => Context.System.EventStream.Publish(new ImportProgressUpdated(
                    command.SessionId,
                    progress.Step,
                    progress.TotalSteps,
                    progress.Message,
                    DateTimeOffset.UtcNow)),
                default);

            Sender.Tell(new PersistProjectImportDataResult(true, result.ProjectId, result.Created, null));
            _log.Info(
                "Persisted import session {0} as project {1} ({2})",
                command.SessionId,
                result.ProjectId,
                result.Created ? "created" : "updated");
        }
        catch (Exception exception)
        {
            _log.Error(exception, "Failed to persist import session {0}", command.SessionId);
            Sender.Tell(new PersistProjectImportDataResult(false, null, false, exception.Message));
        }
    }
}
